using System;
using System.Windows.Forms;

namespace NetGuard
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Branding.Apply(this);

            // عرض لوجو NetGuard فوق العنوان
            try
            {
                picLogo.Image = System.Drawing.Image.FromFile(
                    System.IO.Path.Combine(Application.StartupPath, "Assets", "logo.png"));
            }
            catch { }
        }

        // الكود المسئول عن ضغطة الزرار للانتقال لصفحة تسجيل الدخول
        private void btnStart_Click(object sender, EventArgs e)
        {
            // .1 إنشاء نسخة من الصفحة الثانية (Form2)
            Form2 loginPage = new Form2();
            // .2 إظهار صفحة تسجيل الدخول
            loginPage.Show();
            // .3 إخفاء الصفحة الحالية (الترحيبية)
            this.Hide();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void pnlWelcome_Paint(object sender, PaintEventArgs e)
        {
        }
    }
}