using ICEChecker.ModuleDictionary.Models;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;

namespace ICEChecker.ModuleDictionary.ViewModels {
    public class ShopBRKViewModel : BindableBase {
        public ObservableCollection<BrokerMapping> BRKAbbrs { get; } = new();
        public ICommand EditCommand { get; private set; }
        public ICommand AddCommand { get; private set; }
        public ICommand DeleteCommand { get; private set; }

        private BrokerMapping _selectedBrkRecord;
        public BrokerMapping SelectedBrkRecord {
            get => _selectedBrkRecord;
            set => SetProperty(ref _selectedBrkRecord, value);
            //set => SetProperty(ref _selectedAbbrRecord, value, () => DisplayBtns(value));
        }

        public ShopBRKViewModel() {
            PopulateBrkDictionary();
            EditCommand = new DelegateCommand(Edit);
            AddCommand = new DelegateCommand(Add);
            DeleteCommand = new DelegateCommand(Delete);
        }

        private void Delete() {
        }

        private void Add() {
        }

        private void Edit() {
        }

        private int ID = 1;
        private void PopulateBrkDictionary() {
            try {
                Lookup<string, string> lookUp = (Lookup<string, string>)File.ReadLines(@"config\BrokerDict.csv").Select(line => line.Split(';')).ToLookup(line => line[0], line => line[1]);
                Lookup<string, string> lookUp2 = (Lookup<string, string>)File.ReadLines(@"config\BrokerDict.csv").Select(line => line.Split(';')).ToLookup(line => line[0], line => line[2]);

                // Dictionary<string, string> dict = File.ReadLines(@"config\BrokerDict.csv").Select(line => line.Split(';')).ToDictionary(line => line[0], line => line[1]);
                foreach (var item in lookUp) {
                    foreach (var brk in item) {
                        var abbr = new BrokerMapping {
                            BRKMappingID = ID++,
                            Shop = item.Key,
                            Brk = brk
                        };
                        foreach (var item2 in lookUp2[abbr.Shop]) {
                            abbr.FullName = item2;
                            BRKAbbrs.Add(abbr);
                        }
                    }

                }
            }
            catch (Exception e) { }
        }
    }
}
