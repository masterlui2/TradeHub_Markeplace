using System;
using System.Windows;
using Marketplace_System.Data;

namespace Marketplace_System
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            using AppDbContext dbContext = new();
           
        }
    }
}