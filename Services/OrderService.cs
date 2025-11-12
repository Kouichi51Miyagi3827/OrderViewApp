using Microsoft.Data.SqlClient;
using OrderViewApp.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrderViewApp.Services
{
    public class OrderService
    {
        private readonly string _connectionString;

        public OrderService(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public async Task<List<Order>> GetOrdersAsync()
        {
            var orders = new List<Order>();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                // 受付明細を起点に複数テーブルをJOIN
                // サブクエリをLEFT JOINに変更してクエリを最適化
                var query = @"
                    SELECT 
                        CASE 
                            WHEN [受付明細].[商品コード] = 9814 THEN 
                                CASE 
                                    WHEN [受付明細].[その他商品] IS NOT NULL 
                                         AND LTRIM(RTRIM([受付明細].[その他商品])) != '' 
                                    THEN N'再リース（' + [受付明細].[その他商品] + N'）'
                                    ELSE N'再リース'
                                END
                            ELSE [商品台帳].[商品名]
                        END AS ProductName,
                        [受付明細].[台数] AS Quantity,
                        [受付明細].[摘要] AS Detail,
                        [受付台帳].[期日] AS DueDate,
                        [期限台帳].[期限] AS 期限基本,
                        [受付台帳].[期限2] AS 期限2,
                        [用件台帳].[用件] AS Requirements,
                        [受付台帳].[準備者] AS Preparer,
                        [受付台帳].[出発] AS Departure,
                        [受付明細].[受付番号] AS ReceptionNumber,
                        [受付台帳].[顧客コード] AS CustomerCode,
                        CASE 
                            WHEN [受付台帳].[顧客コード] = 99999 THEN [受付台帳].[顧客名（その他）]
                            ELSE [顧客台帳].[顧客名]
                        END AS CustomerName,
                        ISNULL([StockOK].[Count], 0) AS StockOK,
                        ISNULL([StockCleaning].[Count], 0) AS StockCleaning
                    FROM [OOMIYADB].[dbo].[受付明細]
                    LEFT JOIN [OOMIYADB].[dbo].[受付台帳] 
                        ON [受付明細].[受付番号] = [受付台帳].[受付番号]
                    LEFT JOIN [OOMIYADB].[dbo].[商品台帳] 
                        ON [受付明細].[商品コード] = [商品台帳].[商品コード]
                    LEFT JOIN [OOMIYADB].[dbo].[用件台帳] 
                        ON [受付台帳].[用件コード] = [用件台帳].[用件コード]
                    LEFT JOIN [OOMIYADB].[dbo].[期限台帳] 
                        ON [受付台帳].[期限コード] = [期限台帳].[期限コード]
                    LEFT JOIN [OOMIYADB].[dbo].[顧客台帳] 
                        ON [受付台帳].[顧客コード] = [顧客台帳].[顧客コード]
                    LEFT JOIN (
                        SELECT [商品コード], COUNT(*) AS [Count]
                        FROM [OOMIYADB].[dbo].[ICﾀｸﾞ台帳]
                        WHERE [tag_mode2] = N'ＯＫ'
                        GROUP BY [商品コード]
                    ) AS [StockOK]
                        ON [受付明細].[商品コード] = [StockOK].[商品コード]
                    LEFT JOIN (
                        SELECT [商品コード], COUNT(*) AS [Count]
                        FROM [OOMIYADB].[dbo].[ICﾀｸﾞ台帳]
                        WHERE [tag_mode2] = N'清掃中'
                        GROUP BY [商品コード]
                    ) AS [StockCleaning]
                        ON [受付明細].[商品コード] = [StockCleaning].[商品コード]
                    ORDER BY [受付台帳].[期日] ASC";

                using var command = new SqlCommand(query, connection);
                command.CommandTimeout = 30; // コマンドタイムアウトを設定
                using var reader = await command.ExecuteReaderAsync();

                // カラムのordinalをループ外で一度だけ取得（パフォーマンス最適化）
                var productNameOrdinal = reader.GetOrdinal("ProductName");
                var quantityOrdinal = reader.GetOrdinal("Quantity");
                var detailOrdinal = reader.GetOrdinal("Detail");
                var dueDateOrdinal = reader.GetOrdinal("DueDate");
                var 期限基本Ordinal = reader.GetOrdinal("期限基本");
                var 期限2Ordinal = reader.GetOrdinal("期限2");
                var requirementsOrdinal = reader.GetOrdinal("Requirements");
                var preparerOrdinal = reader.GetOrdinal("Preparer");
                var departureOrdinal = reader.GetOrdinal("Departure");
                var receptionNumberOrdinal = reader.GetOrdinal("ReceptionNumber");
                var customerNameOrdinal = reader.GetOrdinal("CustomerName");
                var stockOKOrdinal = reader.GetOrdinal("StockOK");
                var stockCleaningOrdinal = reader.GetOrdinal("StockCleaning");

                while (await reader.ReadAsync())
                {
                    // 期限の処理（期限基本と期限2を分離）
                    var 期限基本 = reader.IsDBNull(期限基本Ordinal) ? null : reader.GetString(期限基本Ordinal);
                    var 期限2 = reader.IsDBNull(期限2Ordinal) ? null : reader.GetString(期限2Ordinal);
                    
                    // 期限基本（表示用・ソート用）
                    string? expiryDate = 期限基本;
                    
                    // 期限2（表示用）
                    string? expiryDate2 = 期限2;

                    // 台数の型変換（real → int）
                    var quantity = reader.IsDBNull(quantityOrdinal) ? 0 : (int)reader.GetFloat(quantityOrdinal);

                    // ICタグ台帳の集計結果を文字列に変換
                    var stockOK = reader.IsDBNull(stockOKOrdinal) ? "0" : reader.GetInt32(stockOKOrdinal).ToString();
                    var stockCleaning = reader.IsDBNull(stockCleaningOrdinal) ? "0" : reader.GetInt32(stockCleaningOrdinal).ToString();

                    var order = new Order
                    {
                        ProductName = reader.IsDBNull(productNameOrdinal) ? null : reader.GetString(productNameOrdinal),
                        Quantity = quantity,
                        Detail = reader.IsDBNull(detailOrdinal) ? null : reader.GetString(detailOrdinal),
                        DueDate = reader.IsDBNull(dueDateOrdinal) ? DateTime.MinValue : reader.GetDateTime(dueDateOrdinal),
                        ExpiryDate = expiryDate, // 期限基本（表示用・ソート用）
                        ExpiryDate2 = expiryDate2, // 期限2（表示用）
                        Requirements = reader.IsDBNull(requirementsOrdinal) ? null : reader.GetString(requirementsOrdinal),
                        CustomerName = reader.IsDBNull(customerNameOrdinal) ? null : reader.GetString(customerNameOrdinal),
                        ReceptionNumber = reader.IsDBNull(receptionNumberOrdinal) ? (int?)null : reader.GetInt32(receptionNumberOrdinal),
                        StockOK = stockOK,
                        StockCleaning = stockCleaning,
                        Preparer = reader.IsDBNull(preparerOrdinal) ? (short?)null : reader.GetInt16(preparerOrdinal),
                        Departure = reader.IsDBNull(departureOrdinal) ? (bool?)null : reader.GetBoolean(departureOrdinal)
                    };

                    orders.Add(order);
                }
            }
            catch (SqlException ex)
            {
                // エラーログを出力（将来的にはロガーを使用）
                System.Diagnostics.Debug.WriteLine($"データベースエラー: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"予期しないエラー: {ex.Message}");
                throw;
            }

            return orders;
        }
    }
}

