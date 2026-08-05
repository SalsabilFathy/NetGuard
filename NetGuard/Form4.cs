using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Windows.Forms;

namespace NetGuard
{
    public partial class Form4 : Form
    {
        // متغيرات للتحكم في تكرار الصوت
        private bool CriticalAlertActive = false;
        private bool HighAlertActive = false;
        private HashSet<int> alertedDeviceIDs = new HashSet<int>(); // متغير لمنع تكرار رسالة ال AI وتهنيج الجهاز

        public Form4()
        {
            InitializeComponent();
            Branding.Apply(this);
        }

        //تشغيل التايمر للتحديث التلقائي
        private void Form4_Load(object sender, EventArgs e)
        {
            if (!this.DesignMode) //لمنع الخطأ في ال Designer
            {
                alertedDeviceIDs.Clear(); // تصفير المتغير لضمان عمل التنبيه مع كل Run جديد
                dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                LoadAlertsData(); //جلب البيانات لاول مرة
                RunPredictiveAI(); // تشغيل ال AI فورا عند التشغيل
                timer1.Interval = 5000; //التحديث كل 5 ثواني
                timer1.Start(); //تشغيل التايمر
            }
        }

        //دالة جلب البيانات من قاعدة البيانات باستخدام JOIN للربط بين الجداول
        private void LoadAlertsData()
        {
            //جلب رابط الاتصالل من ملف App.config
            string connString = ConfigurationManager.ConnectionStrings["Myconn"].ConnectionString;

            //استعلام SQL يربط التنبيهات بأنواعها و بمستويات خطورتها
            string query = @"SELECT A.AlertID, A.Message, T.TypeName AS [Alert_Type],
                     S.SeverityName AS [Severity_Level], A.TimeStamp
                     FROM Alerts A
                     JOIN AlertType T ON A.AlertTypeID_FK = T.AlertTypeID
                     JOIN Severity S ON A.SeverityID_FK = S.SeverityID
                     ORDER BY A.TimeStamp DESC";

            using (SqlConnection conn = new SqlConnection(connString))
            {
                try
                {
                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt); //تعبئة البيانات في الجدول

                    // لو مفيش تنبيهات Critical/High دلوقتي، نسمح للصوت يشتغل تاني لو ظهرت من جديد
                    bool hasCritical = dt.AsEnumerable().Any(r => r["Severity_Level"].ToString().Trim() == "Critical");
                    bool hasHigh = dt.AsEnumerable().Any(r => r["Severity_Level"].ToString().Trim() == "High");
                    if (!hasCritical) CriticalAlertActive = false;
                    if (!hasHigh) HighAlertActive = false;

                    dataGridView1.DataSource = dt;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("خطأ في الاتصال بقاعدة البيانات: " + ex.Message);
                }
            }
        }

        //الحدث المسؤول عن تلوين الخلايا بناءً علي قيمة مستوي الخطورة
        private void dataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            //التأكد من اننا نتعامل مع عمود مستوي الخطورة فقط
            if (dataGridView1.Columns[e.ColumnIndex].Name == "Severity_Level")
            {
                // نستخدم Trim() لإزالة أي مسافات زائدة قد تمنع التلوين
                string status = e.Value?.ToString().Trim();

                //تطبيق الالوان بناءً علي مستوي الخطورة
                if (status == "Critical")
                {
                    e.CellStyle.BackColor = Color.Red; //الأحمر للخطر الأقصي
                    e.CellStyle.ForeColor = Color.White;

                    // تشغيل صوت التنبيه القوي مرة واحدة فقط عند ظهور الحالة
                    if (!CriticalAlertActive)
                    {
                        SystemSounds.Hand.Play();
                        CriticalAlertActive = true;
                    }
                }
                else if (status == "High")
                {
                    e.CellStyle.BackColor = Color.DarkOrange; //البرتقالي للخطورة العالية

                    // تنبيه صوتي أخف للحالات العالية مرة واحدة
                    if (!HighAlertActive)
                    {
                        SystemSounds.Beep.Play();
                        HighAlertActive = true;
                    }
                }
                else if (status == "Medium")
                {
                    e.CellStyle.BackColor = Color.Yellow; //الأصفر للخطورة المتوسطة
                    e.CellStyle.ForeColor = Color.Black;
                }
                else if (status == "Low")
                {
                    e.CellStyle.BackColor = Color.Green; //الأخضر للخطورة المنخفضة
                    e.CellStyle.ForeColor = Color.White;
                }
            }
        }

        // --- الجزء الثالث: محرك التنبؤ الذكي AI Predictive Engine (شغل العضو الثالث) ---
        private void RunPredictiveAI()
        {
            string connString = ConfigurationManager.ConnectionStrings["Myconn"].ConnectionString;

            // Logic: فحص الأجهزة التي تكررت أعطالها (High/Critical) أكثر من 3 مرات في آخر 24 ساعة
            string aiQuery = @"SELECT DeviceID_FK, COUNT(*) as IncidentCount 
                               FROM Alerts 
                               WHERE SeverityID_FK >= 3 
                               AND TimeStamp >= DATEADD(day, -1, GETDATE())
                               GROUP BY DeviceID_FK 
                               HAVING COUNT(*) >= 3";
            using (SqlConnection conn = new SqlConnection(connString))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(aiQuery, conn);
                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    HashSet<int> currentIssues = new HashSet<int>();

                    while (reader.Read())  // استخدام while للمرور علي جميع الأجهزة المشتبه بها
                    {
                        int devID = Convert.ToInt32(reader["DeviceID_FK"]);
                        int count = Convert.ToInt32(reader["IncidentCount"]);
                        currentIssues.Add(devID);

                        // الذكاء: لا تظهر الرسالة إذا كنت قد حذرت من نفس الجهاز مسبقاً (يمنع التعليق)
                        if (!alertedDeviceIDs.Contains(devID))
                        {
                            alertedDeviceIDs.Add(devID);
                            NotifyAIRecommendation(devID, count);
                        }
                    }

                    // أي جهاز رجع طبيعي (مبقاش من ضمن الأجهزة المشتبه فيها)، نشيله من القائمة
                    // عشان لو اتعطل تاني في المستقبل يتنبه عليه من جديد
                    alertedDeviceIDs.IntersectWith(currentIssues);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("AI Error:" + ex.Message);
                }
            }
        }

        private void NotifyAIRecommendation(int deviceID, int incidents)
        {
            string msg = $"⚠️ [AI Insights]: نمط غير مستقر للجهاز رقم ({deviceID}).\n" +
                          $"تم رصد {incidents} أعطال خطيرة خلال 24 ساعة.\n" +
                          "التوصية: فحص التوصيلات فوراً لتجنب انهيار الشبكة.";

            SystemSounds.Exclamation.Play();
            MessageBox.Show(msg, "AI Predictive Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        //التايمر الذي يقوم باستدعاء دالة تحديث البيانات دورياً
        private void timer1_Tick(object sender, EventArgs e)
        {
            // بنحدث البيانات و بنشغل ال AI بس و التحكم في الصوت داخل ال Grid
            LoadAlertsData();
            RunPredictiveAI();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
        }

        private void btnOpenDashboard_Click(object sender, EventArgs e)
        {
            Form5 dashboard = new Form5();
            // إظهار الشاشة
            dashboard.Show();
        }
    }
}