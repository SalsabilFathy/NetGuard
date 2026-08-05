using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace NetGuard
{
    // كلاس مساعد بيوحد شكل الهوية البصرية (الأيقونة + الخلفية) على كل فورمز المشروع
    internal static class Branding
    {
        public static void Apply(Form form)
        {
            try
            {
                string assetsPath = Path.Combine(Application.StartupPath, "Assets");

                // تغيير أيقونة النافذة اللي بتظهر جمب اسم الفورم في شريط العنوان
                string iconPath = Path.Combine(assetsPath, "logo.ico");
                if (File.Exists(iconPath))
                {
                    form.Icon = new Icon(iconPath);
                }

                // خلفية الفورم بتيمة الشبكات
                string bgPath = Path.Combine(assetsPath, "background.png");
                if (File.Exists(bgPath))
                {
                    form.BackgroundImage = Image.FromFile(bgPath);
                    form.BackgroundImageLayout = ImageLayout.Stretch;
                }
            }
            catch
            {
                // لو حصل أي خطأ في تحميل الأصول، الفورم تفتح عادي من غير ما تقف
            }
        }
    }
}
