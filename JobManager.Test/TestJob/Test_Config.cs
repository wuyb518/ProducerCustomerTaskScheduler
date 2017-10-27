namespace JobManager.Test.TestJob
{
    public static class Test_Config
    {
    

        public static string Redis_Server { get; set; } = "127.0.0.1:6379";
        public static int Redis_DBIndex { get; set; } = 10;

        public static string JobList_Queue_Name { get; set; } = "article_label_analyze_joblist_queue";
        public static string JobList_Hash_Name { get; set; } = "article_label_analyze_joblist_hash";
        public static string JobResultList_Hash_Name { get; set; } = "article_label_analyze_jobresultlist_hash";

        public static string Job_Customer_Process_Count = "article_label_analyze_job_customer_process_count";
        public static string Job_Customer_Success_Count = "article_label_analyze_job_customer_success_count";

        public static string JobBusiType { get; set; } = "article_label_analyze";

    }
}
