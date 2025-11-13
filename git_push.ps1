# Gitリポジトリの初期化とプッシュスクリプト
$ErrorActionPreference = "Stop"

# プロジェクトディレクトリに移動
$projectPath = "\\OOMIYASV1\ohmiya\個人用ファイル\宮城宏一\OrderViewApp"
Set-Location $projectPath

Write-Host "現在のディレクトリ: $(Get-Location)" -ForegroundColor Green

# Gitリポジトリが初期化されているか確認
if (-not (Test-Path ".git")) {
    Write-Host "Gitリポジトリを初期化しています..." -ForegroundColor Yellow
    git init
}

# ネットワーク共有ディレクトリを安全なディレクトリとして追加
Write-Host "ネットワーク共有ディレクトリを安全なディレクトリとして設定しています..." -ForegroundColor Yellow
$safeDir = "%(prefix)///Oomiyasv1/ohmiya/個人用ファイル/宮城宏一/OrderViewApp"
git config --global --add safe.directory $safeDir
Write-Host "安全なディレクトリとして設定しました。" -ForegroundColor Green

# Gitユーザー設定の確認と設定
Write-Host "`nGitユーザー設定を確認しています..." -ForegroundColor Yellow
$userName = git config --global user.name 2>$null
$userEmail = git config --global user.email 2>$null

if (-not $userName -or -not $userEmail) {
    Write-Host "Gitユーザー情報が設定されていません。設定します..." -ForegroundColor Yellow
    if (-not $userName) {
        git config --global user.name "Kouichi51Miyagi3827"
        Write-Host "ユーザー名を設定しました: Kouichi51Miyagi3827" -ForegroundColor Green
    }
    if (-not $userEmail) {
        # GitHubのnoreplyメールアドレスを使用
        git config --global user.email "Kouichi51Miyagi3827@users.noreply.github.com"
        Write-Host "メールアドレスを設定しました: Kouichi51Miyagi3827@users.noreply.github.com" -ForegroundColor Green
    }
} else {
    Write-Host "Gitユーザー情報は既に設定されています。" -ForegroundColor Green
    Write-Host "  ユーザー名: $userName" -ForegroundColor Cyan
    Write-Host "  メールアドレス: $userEmail" -ForegroundColor Cyan
}

# リモートリポジトリの確認と設定
$remoteUrl = "https://github.com/Kouichi51Miyagi3827/OrderViewApp.git"
$remote = git remote -v 2>$null
if (-not $remote) {
    Write-Host "リモートリポジトリを追加しています: $remoteUrl" -ForegroundColor Yellow
    git remote add origin $remoteUrl
    Write-Host "リモートリポジトリを追加しました。" -ForegroundColor Green
} else {
    # 既存のリモートを確認
    $existingUrl = git remote get-url origin 2>$null
    if ($existingUrl -ne $remoteUrl) {
        Write-Host "リモートURLを更新しています..." -ForegroundColor Yellow
        git remote set-url origin $remoteUrl
        Write-Host "リモートURLを更新しました。" -ForegroundColor Green
    } else {
        Write-Host "リモートリポジトリは既に設定されています。" -ForegroundColor Green
    }
}

# ステータス確認
Write-Host "`nGitステータスを確認しています..." -ForegroundColor Yellow
git status

# すべてのファイルをステージング
Write-Host "`nファイルをステージングしています..." -ForegroundColor Yellow
git add .

# コミット（既にコミット済みの場合はスキップ）
$hasChanges = git diff --cached --quiet
if ($LASTEXITCODE -eq 0) {
    Write-Host "ステージングされた変更がありません。既にコミット済みの可能性があります。" -ForegroundColor Yellow
} else {
    Write-Host "コミットしています..." -ForegroundColor Yellow
    git commit -m "Initial commit"
    if ($LASTEXITCODE -ne 0) {
        Write-Host "コミットに失敗しました。既にコミット済みの可能性があります。" -ForegroundColor Yellow
    }
}

# プッシュ
Write-Host "`nリモートリポジトリにプッシュしています..." -ForegroundColor Yellow
$branch = git branch --show-current
if (-not $branch) {
    git branch -M main
    $branch = "main"
}

try {
    git push -u origin $branch
    Write-Host "`nプッシュが完了しました！" -ForegroundColor Green
} catch {
    Write-Host "`nプッシュ中にエラーが発生しました: $_" -ForegroundColor Red
    Write-Host "手動で 'git push -u origin $branch' を実行してください。" -ForegroundColor Yellow
}

