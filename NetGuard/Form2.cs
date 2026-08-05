using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace NetGuard
{
    public partial class Form2 : Form
    {
        // تعريف عداد المحاولات خارج الدالة عشان يحافظ على قيمته
        int loginAttempts = 0;

        public Form2()
        {
            InitializeComponent();
            Branding.Apply(this);
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            // .1 نص الاتصال بقاعدة البيانات
            string connectionString = ConfigurationManager.ConnectionStrings["Myconn"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    // .2 أولاً: نتحقق إذا كان اسم المستخدم موجود أصلاً
                    string checkUserQuery = "SELECT Password FROM Users WHERE UserName=@user";
                    SqlCommand cmdUser = new SqlCommand(checkUserQuery, conn);
                    cmdUser.Parameters.AddWithValue("@user", txtUserName.Text.Trim());
                    object dbPassword = cmdUser.ExecuteScalar();

                    if (dbPassword == null)
                    {
                        // الاسم غير موجود في القاعدة
                        loginAttempts++;
                        ShowLoginError("اسم المستخدم غير صحيح!");
                    }
                    else
                    {
                        // الاسم موجود، نتحقق الآن من كلمة المرور
                        if (dbPassword.ToString() == txtPassword.Text.Trim())
                        {
                            // الدخول ناجح
                            MessageBox.Show("تم تسجيل الدخول بنجاح! جاري فتح نظام المراقبة...", "نجاح",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            Form3 f3 = new Form3();
                            f3.Show();
                            this.Hide();
                        }
                        else
                        {
                            // الباسورد غلط
                            loginAttempts++;
                            ShowLoginError("كلمة المرور غير صحيحة!");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("خطأ في الاتصال بقاعدة البيانات: " + ex.Message);
                }
            }
        }

        // دالة مساعدة لإظهار الرسائل والتحقق من عدد المحاولات
        private void ShowLoginError(string specificError)
        {
            if (loginAttempts >= 3)
            {
                MessageBox.Show(specificError + $"\nتم استهلاك جميع المحاولات ({loginAttempts}/3).\nتم قفل النظام!",
                    "تنبيه أمني", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                btnLogin.Enabled = false; // تعطيل زر الدخول نهائياً
            }
            else
            {
                MessageBox.Show(specificError + $"\nعدد المحاولات الحالية: {loginAttempts} من 3",
                    "خطأ في الدخول", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void Form2_Load(object sender, EventArgs e)
        {
        }

        private void txtPassword_TextChanged(object sender, EventArgs e)
        {
        }

        private void pnlCard_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}