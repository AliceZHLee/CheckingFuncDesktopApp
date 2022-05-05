using ICEChecker.ModuleCheckerOutput.Models;
using Prism.Events;
using Prism.Mvvm;
using System.Collections.ObjectModel;

namespace ICEChecker.ModuleCheckerOutput.ViewModels {
    public class OutputViewModel : BindableBase {
        private IEventAggregator _ea;
        //public CollectionViewSource InconsistentOutputSource { get; } = new();
        //public CollectionViewSource ConsistentOutputSource { get; } = new();
        public ObservableCollection<CheckerOutput> ConsistentOutputs { get; } = new();
        public ObservableCollection<CheckerOutput> InconsistentOutputs { get; } = new();

        private CheckerOutput _selectedInconsistentRecord;
        public CheckerOutput SelectedInconsistentRecord {
            get => _selectedInconsistentRecord;
            set => SetProperty(ref _selectedInconsistentRecord, value, () => FindSimilarRecord());
        }

        private CheckerOutput _selectedConsistentRecord;
        public CheckerOutput SelectedConsistentRecord {
            get => _selectedConsistentRecord;
            set => SetProperty(ref _selectedConsistentRecord, value, () => FindSameRecord());
        }

        private void FindSameRecord() {
            _ea.GetEvent<FindSameRecordEvent>().Publish(SelectedConsistentRecord);
        }

        private void FindSimilarRecord() {
            _ea.GetEvent<FindSimilarRecordEvent>().Publish(SelectedInconsistentRecord);
        }

        public OutputViewModel(IEventAggregator ea) {
            _ea = ea;
            //ConsistentOutputSource.Source = ConsistentOutputs;
            //InconsistentOutputSource.Source = InconsistentOutputs;

            _ea.GetEvent<AddOutputEvent>().Subscribe(AddOutputToView);
        }
        private int _consistNum;
        public int ConsistNum {
            get=> _consistNum;
            set => SetProperty(ref _consistNum, value);
        }
        private int _inconsistNum;
        public int InconsistNum {
            get => _inconsistNum;
            set => SetProperty(ref _inconsistNum, value);
        }

        private void AddOutputToView(CheckerOutput outcome) {
            if (outcome.IsConsistent) {
                ConsistentOutputs.Insert(0,outcome);
                ConsistNum = ConsistentOutputs.Count;
            }
            else {
                InconsistentOutputs.Insert(0,outcome);
                InconsistNum = InconsistentOutputs.Count;

            }
        }
    }
}
