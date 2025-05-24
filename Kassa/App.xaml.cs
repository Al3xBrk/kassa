using System.Configuration;
using System.Data;
using System.Windows;
using Microsoft.EntityFrameworkCore;

namespace Kassa
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            using (var db = new KassaContext())
            {
                db.Database.Migrate(); // Автоматически применяет миграции и создаёт базу при первом запуске
            }
        }
    }

}
