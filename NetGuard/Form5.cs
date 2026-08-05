using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Reflection.Emit;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;

namespace NetGuard
{
    public partial class Form5 : Form
    {
        // سطر الاتصال بتاعك
        string connString = ConfigurationManager.ConnectionStrings["Myconn"].ConnectionString;

        public Form5()
        {
            InitializeComponent();
            Branding.Apply(this);
        }

        private void Form5_Load(object sender, EventArgs e)
        {
            // ضبط النصوص للأسماء الافتراضية اللي سبتها
            this.Text = "NetGuard - لوحة التحكم الذكية";
            groupBox1.Text = "إحصائيات الأجهزة";
            groupBox2.Text = "تحليل أداء الشبكة";
            groupBox3.Text = "مركز التوصيات الذكية (AI)";
            label1.Text = "الأجهزة النشطة: 4\nالأجهزة المعطلة: 0";
            label2.Text = "نظام التحليل التنبؤي في وضع السكون...\nبانتظار اكتمال بيانات الاختبار لتوليد التوصيات.";
            label2.ForeColor = Color.LightGray;

            // تشغيل الرسم البياني
            SetupPieChart();
            LoadChartData();
        }

        private void SetupPieChart()
        {
            chart1.Series.Clear();
            chart1.Titles.Clear();
            chart1.Titles.Add("توزيع مستويات الخطورة");

            Series s = chart1.Series.Add("Severities");
            s.ChartType = SeriesChartType.Pie; // الدائرة
            s.IsValueShownAsLabel = true;
        }

        private void LoadChartData()
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                try
                {
                    // الأسماء هنا الزم تكون زي الـ SQL بتاعك بالظبط
                    string query = @"SELECT S.SeverityName, COUNT(A.AlertID) as Total
                                     FROM Severity S
                                     LEFT JOIN Alerts A ON S.SeverityID = A.SeverityID_FK
                                     GROUP BY S.SeverityName";

                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    foreach (DataRow row in dt.Rows)
                    {
                        if (Convert.ToInt32(row["Total"]) > 0)
                        {
                            string severityName = row["SeverityName"].ToString().Trim();
                            int pointIndex = chart1.Series["Severities"].Points.AddXY(severityName, row["Total"]);

                            // نفس ألوان مستويات الخطورة المستخدمة في شاشة التنبيهات (Form4) بالظبط
                            System.Drawing.Color severityColor;
                            switch (severityName)
                            {
                                case "Critical":
                                    severityColor = System.Drawing.Color.Red;
                                    break;
                                case "High":
                                    severityColor = System.Drawing.Color.DarkOrange;
                                    break;
                                case "Medium":
                                    severityColor = System.Drawing.Color.Yellow;
                                    break;
                                case "Low":
                                    severityColor = System.Drawing.Color.Green;
                                    break;
                                default:
                                    severityColor = System.Drawing.Color.Gray;
                                    break;
                            }
                            chart1.Series["Severities"].Points[pointIndex].Color = severityColor;
                        }
                    }
                }
                catch (Exception ex)
                {
                    // لو طلع Error هيظهر هنا في الـ Output بتاع الفيجوال ستوديو
                    System.Diagnostics.Debug.WriteLine("Dashboard Error: " + ex.Message);
                }
            }
        }
    }
}