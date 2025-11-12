namespace OrderViewApp.Models
{
    /// <summary>
    /// 表示モードの種類
    /// </summary>
    public enum DisplayModeType
    {
        /// <summary>
        /// 未完了（準備者 = 0）
        /// </summary>
        Incomplete,
        
        /// <summary>
        /// 完了含む（すべてのデータ）
        /// </summary>
        All,
        
        /// <summary>
        /// 完了のみ（準備者 ≠ 0）
        /// </summary>
        Completed
    }
}

