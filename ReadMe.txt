データベース構造とJOIN関係
1. 受付明細テーブル ([OOMIYADB].[dbo].[受付明細]) - メインテーブル
取得するカラム:

受付番号 (int, FK) → 受付台帳とJOIN
商品コード (int, FK) → 商品台帳とJOIN、ICタグ台帳とJOIN
台数 (real) → Order.Quantity にマッピング
摘要 (nvarchar(30)) → Order.Detail にマッピング
2. 受付台帳テーブル ([OOMIYADB].[dbo].[受付台帳])
受付番号 (int, PK) → 受付明細の受付番号とJOIN
期日 (datetime) → Order.DueDate にマッピング
期限コード (smallint) → 期限台帳とJOIN
期限2 (nvarchar(20)) → 期限表示の補足情報
用件コード (smallint) → 用件台帳とJOIN
3. 商品台帳テーブル ([OOMIYADB].[dbo].[商品台帳])
商品コード (int, PK) → 受付明細の商品コードとJOIN
商品名 (nvarchar(255)) → Order.ProductName にマッピング
4. 用件台帳テーブル ([OOMIYADB].[dbo].[用件台帳])
用件コード (smallint, PK) → 受付台帳の用件コードとJOIN
用件 (nvarchar(20)) → Order.Requirements にマッピング
5. 期限台帳テーブル ([OOMIYADB].[dbo].[期限台帳])
期限コード (smallint, PK) → 受付台帳の期限コードとJOIN
期限 (nvarchar(20)) → 期限の基本文字列（例: "10時"）
6. ICタグ台帳テーブル ([OOMIYADB].[dbo].[ICﾀｸﾞ台帳])
商品コード (推測: int, FK) → 受付明細の商品コードとJOIN（要確認）
tag_mode2 (nvarchar) → 在庫状態を表す
値: NULL, 在庫, 自社使用, 整備中, 修理中, 清掃中, ＯＫ, 貸出中, 廃棄, 予約, 登録
「ＯＫ」の数をカウント → Order.StockOK
「清掃中」の数をカウント → Order.StockCleaning

データマッピング
| Orderプロパティ | データソース | 備考 |

|----------------|------------|------|

| ProductName | 商品台帳.商品名 | 商品コードでJOIN |

| Quantity | 受付明細.台数 | real型をintに変換 |

| Detail | 受付明細.摘要 | |

| DueDate | 受付台帳.期日 | 期日でASCソート |

| ExpiryDate | 期限台帳.期限 + 受付台帳.期限2 | 文字列結合（文字列として扱う） |

| Requirements | 用件台帳.用件 | 用件コードでJOIN |

| Priority | 計算値 | データベースから直接取得せず、後で表示内容を決定（現時点ではNULL） |

| StockOK | ICタグ台帳.tag_mode2の集計 | 「ＯＫ」の数をカウント（文字列として表示） |

| StockCleaning | ICタグ台帳.tag_mode2の集計 | 「清掃中」の数をカウント（文字列として表示） |

期限の表示ロジック
期限台帳.期限 + 受付台帳.期限2 = 表示する期限
例: 期限台帳の期限が「10時」、期限2が「頃」→ 表示は「10時頃」
NULL値の処理:
期限2がNULLの場合: 期限台帳の期限のみ表示
期限台帳の期限がNULLの場合: 期限2のみ表示（または空文字列）
両方NULLの場合: 空文字列を表示
実装内容
1. Orderモデルの更新
ExpiryDateをDateTimeからstringに変更（期限は文字列として表示）
StockOKとStockCleaningは既にstring?型なので、数値を文字列として設定
2. OrderService.csのSQLクエリ更新
複数テーブルをLEFT JOIN:

受付明細 (メイン)
受付台帳 (受付番号でJOIN)
商品台帳 (商品コードでJOIN)
用件台帳 (用件コードでJOIN、受付台帳から)
期限台帳 (期限コードでJOIN、受付台帳から)
ICタグ台帳の集計（サブクエリで実装）
期日でASCソート
3. ICタグ台帳の集計処理
受付明細の各レコードに対して、商品コードでICタグ台帳を参照
tag_mode2が「ＯＫ」のレコード数をカウント → StockOK
tag_mode2が「清掃中」のレコード数をカウント → StockCleaning
集計方法: サブクエリでCOUNT(*)を使用
4. データマッピング処理
台数 (real) → Quantity (int) への型変換
期限の文字列結合処理（C#側で実装）
ICタグ台帳の集計結果（int）を文字列に変換
5. NULL値の処理
各JOINでNULLが発生する可能性を考慮
期限の結合時のNULL処理
ICタグ台帳の集計結果が0の場合も適切に表示
6. 優先度カラムの処理
データベースから直接取得しない
初期値はNULLまたは空文字列
後で表示内容を決定するため、現時点では実装しない
7. MainWindow.xamlの更新
期限カラムのStringFormatを削除（文字列なので不要）
優先度カラムは現時点ではそのまま（後で実装）

SQLクエリ案
SELECT 
    [商品台帳].[商品名] AS ProductName,
    [受付明細].[台数] AS Quantity,
    [受付明細].[摘要] AS Detail,
    [受付台帳].[期日] AS DueDate,
    [期限台帳].[期限] AS 期限基本,
    [受付台帳].[期限2] AS 期限2,
    [用件台帳].[用件] AS Requirements,
    (SELECT COUNT(*) 
     FROM [OOMIYADB].[dbo].[ICﾀｸﾞ台帳] 
     WHERE [ICﾀｸﾞ台帳].[商品コード] = [受付明細].[商品コード] 
       AND [ICﾀｸﾞ台帳].[tag_mode2] = N'ＯＫ') AS StockOK,
    (SELECT COUNT(*) 
     FROM [OOMIYADB].[dbo].[ICﾀｸﾞ台帳] 
     WHERE [ICﾀｸﾞ台帳].[商品コード] = [受付明細].[商品コード] 
       AND [ICﾀｸﾞ台帳].[tag_mode2] = N'清掃中') AS StockCleaning
FROM [OOMIYADB].[dbo].[受付明細]
LEFT JOIN [OOMIYADB].[dbo].[受付台帳] 
    ON [受付明細].[受付番号] = [受付台帳].[受付番号]
LEFT JOIN [OOMIYADB].[dbo].[商品台帳] 
    ON [受付明細].[商品コード] = [商品台帳].[商品コード]
LEFT JOIN [OOMIYADB].[dbo].[用件台帳] 
    ON [受付台帳].[用件コード] = [用件台帳].[用件コード]
LEFT JOIN [OOMIYADB].[dbo].[期限台帳] 
    ON [受付台帳].[期限コード] = [期限台帳].[期限コード]
ORDER BY [受付台帳].[期日] ASC


未完了: 準備者 = 0 のデータのみ表示
完了含む: すべてのデータを表示
完了のみ: 準備者 ≠ 0 のデータのみ表示