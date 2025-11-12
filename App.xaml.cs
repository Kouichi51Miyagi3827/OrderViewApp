using Microsoft.Extensions.Configuration;
using System.IO;
using System.Windows;

namespace OrderViewApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IConfiguration? Configuration { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // appsettings.jsonを読み込む（起動時のパフォーマンス向上のためreloadOnChangeをfalseに設定）
            // PublishSingleFile=trueの場合、AppContext.BaseDirectoryを使用して実行ファイルの場所を取得
            var basePath = AppContext.BaseDirectory;
            var builder = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);

            Configuration = builder.Build();
        }

        public static string GetConnectionString()
        {
            return Configuration?.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("接続文字列が見つかりません。appsettings.jsonを確認してください。");
        }
    }

}
