using ICEChecker.Core;
using ICEChecker.ModuleCheckerInput.Models;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System.Collections.Generic;
using System.Windows.Input;

namespace ICEChecker.LogonWindow.ViewModels {
    public class LogonViewModel:BindableBase {
        private IEventAggregator _ea;
        public List<string> AccountOptions { get; } = new();
        public ICommand UpdateUserInfoCommand { get; private set; }
        public static bool FirstTimeLogin;
        public static string UserID;

        public LogonViewModel(IEventAggregator ea) {
            _ea = ea;
            UpdateUserInfoCommand = new DelegateCommand<string>(updateLoginSetting);
            LoadUserSetting();
        }

        private void updateLoginSetting(string userID) {
            ConfigHelper.UpdateUserSetting(@"config\config.json", userID);
            _ea.GetEvent<RetrieveUserIDEvent>().Publish(userID);
        }

        private void LoadUserSetting() {
            //add trader accounts
            foreach (var account in ConfigHelper.LoadAccountData(@"config\config.json")) {
                AccountOptions.Add(account);
            }
            var loginSetting = ConfigHelper.LoadLoginSetting(@"config\config.json");
            UserID = loginSetting["UserID"].ToString();
            FirstTimeLogin = (bool)loginSetting["FirstTimeLogin"];
            if (!FirstTimeLogin) {
                _ea.GetEvent<RetrieveUserIDEvent>().Publish(UserID);
            }
        }
    }
}
