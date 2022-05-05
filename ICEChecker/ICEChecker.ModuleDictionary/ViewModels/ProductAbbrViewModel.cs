using ICEChecker.ModuleDictionary.Models;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ICEChecker.ModuleDictionary.ViewModels {
    public class ProductAbbrViewModel:BindableBase {
        public ObservableCollection<AbbrMapping> ProductAbbrs { get; } = new();
        public ICommand EditCommand { get; private set; }
        public ICommand AddCommand { get; private set; }
        public ICommand DeleteCommand { get; private set; }

        private AbbrMapping _selectedAbbrRecord;
        public AbbrMapping SelectedAbbrRecord {
            get => _selectedAbbrRecord;
            set => SetProperty(ref _selectedAbbrRecord, value);
            //set => SetProperty(ref _selectedAbbrRecord, value, () => DisplayBtns(value));
        }

        public ProductAbbrViewModel() {
            PopulateProductAbbrDictionary();
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
        private void PopulateProductAbbrDictionary() {
            try {
                Dictionary<string, string> dict = File.ReadLines(@"config\AbbrDict.csv").Select(line => line.Split(';')).ToDictionary(line => line[0], line => line[1]);
                foreach (var item in dict) {
                    var abbr = new AbbrMapping {
                        AbbrMappingID= ID++,
                        ProductName= item.Key,
                        Abbr=item.Value
                    };
                    ProductAbbrs.Add(abbr);
                }
            }
            catch (Exception e) { }
        }
    }
}
