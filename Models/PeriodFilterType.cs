namespace OrderViewApp.Models
{
    /// <summary>
    /// 期間フィルタリングの種類
    /// </summary>
    public enum PeriodFilterType
    {
        /// <summary>
        /// 当日
        /// </summary>
        Today,
        
        /// <summary>
        /// 当日＆翌日
        /// </summary>
        TodayAndTomorrow,
        
        /// <summary>
        /// 翌日
        /// </summary>
        Tomorrow,
        
        /// <summary>
        /// カスタム期間
        /// </summary>
        Custom
    }
}

