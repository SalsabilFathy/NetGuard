using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Reflection.Emit;
using System.Windows.Forms;


namespace NetGuard
{
    public partial class Form3 : Form
    {
        // سطر الاتصال - اتأكدي إن اسم قاعدة البيانات NetGuard
        string connString = ConfigurationManager.ConnectionStrings["Myconn"].ConnectionString;

        public Form3()
        {
            InitializeComponent();
            Branding.Apply(this);
            // أول ما الصفحة تفتح، نحاول نعرض البيانات فوراً
            LoadDataToGrid();
            if (button2 != null) button2.Enabled = false;
        }

        // .1 ميثود جلب البيانات (معدلة لتجبر الجدول على الظهور)
        private void LoadDataToGrid()
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    // استعلام مطابق للصورة اللي بعتيها
                    string query = "SELECT DeviceID, IpAddress, DeviceName, Status, CreateDate FROM Devices";
                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    if (dataGridView1 != null)
                    {
                        dataGridView1.DataSource = null; // تصفير الجدول
                        dataGridView1.DataSource = dt;    // ربط البيانات الجديدة
                        // تعديل شكل الجدول عشان يملى الشاشة ويبقى احترافي
                        dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                        label2.Text = "تم تحديث البيانات: " + DateTime.Now.ToString("HH:mm:ss");
                        label2.ForeColor = Color.SpringGreen;
                    }
                }
                catch (Exception ex)
                {
                    if (label2 != null) label2.Text = "فشل جلب البيانات";
                    MessageBox.Show("السبب في عدم ظهور الجدول: " + ex.Message);
                }
            }
        }

        // .2 زرار بدء المراقبة (button1)
        private void button1_Click(object sender, EventArgs e)
        {
            timer1.Interval = 5000;
            timer1.Start();
            button1.Enabled = false;
            button2.Enabled = true;
            label2.Text = "بدأت المراقبة الآن...";
            label2.ForeColor = Color.Cyan;
            LoadDataToGrid(); // تحديث فوري عند الضغط
        }

        // .3 زرار إنهاء المراقبة (button2)
        private void button2_Click(object sender, EventArgs e)
        {
            timer1.Stop();
            button1.Enabled = true;
            button2.Enabled = false;
            label2.Text = "تم إيقاف المراقبة";
            label2.ForeColor = Color.Red;
        }

        // .4 زرار سجل التنبيهات (button3)
        private void button3_Click(object sender, EventArgs e)
        {
            Form4 f4 = new Form4();
            f4.Show();
        }

        // .5 حدث التايمر (timer1_Tick)
        private void timer1_Tick(object sender, EventArgs e)
        {
            LoadDataToGrid();
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            // بنعمل نسخة من الفورم الرابع
            Form4 f4 = new Form4();
            // بنظهر الفورم الرابع للمستخدم
            f4.Show();
        }

        private void dataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            // بنشوف لو إحنا في العمود اللي اسمه "Status"
            if (dataGridView1.Columns[e.ColumnIndex].Name == "Status" && e.Value != null)
            {
                string statusValue = e.Value.ToString();
                // لو الحالة Active (شغال) يلون الصف بالأخضر
                if (statusValue == "Active")
                {
                    e.CellStyle.BackColor = Color.Green;
                    e.CellStyle.ForeColor = Color.White;
                }
                // لو الحالة Offline أو فيها عطل يلون بالأحمر
                else if (statusValue == "Offline")
                {
                    e.CellStyle.BackColor = Color.Red;
                    e.CellStyle.ForeColor = Color.White;
                }
                // لو الحالة صيانة (Maintenance) يلون بالبرتقالي مثلاً
                else if (statusValue == "Maintenance")
                {
                    e.CellStyle.BackColor = Color.Orange;
                    e.CellStyle.ForeColor = Color.Black;
                }
            }
        }

        private void Form3_Load(object sender, EventArgs e)
        {
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }
    }
}