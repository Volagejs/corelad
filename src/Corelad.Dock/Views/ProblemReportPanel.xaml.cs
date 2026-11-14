using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Corelad.Dock.ViewModels;

namespace Corelad.Dock.Views
{
    /// <summary>
    /// Sorun raporu paneli. Yalnızca sunum ve klavye yönlendirmesi yapar; COM çağrısı,
    /// şekil seçimi veya geometri hesabı içermez (AGENTS.md §2).
    ///
    /// Kullanım: <see cref="ViewModel"/> özelliğine bir <see cref="ProblemReportViewModel"/>
    /// atanır (genellikle analiz sonrası <c>Load</c> ile doldurulur). Kontur vurgusu için
    /// ViewModel'in <c>HighlightRequested</c> olayına veya <c>HighlightSink</c> arayüzüne
    /// bağlanılır; panel CorelDRAW'ı doğrudan çağırmaz.
    /// </summary>
    public partial class ProblemReportPanel : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            nameof(ViewModel),
            typeof(ProblemReportViewModel),
            typeof(ProblemReportPanel),
            new PropertyMetadata(null));

        public ProblemReportPanel()
        {
            InitializeComponent();

            // Tasarım zamanı ve ilk açılış için boş model: "Henüz rapor yok" durumu gösterilir.
            ViewModel = new ProblemReportViewModel();
        }

        public ProblemReportViewModel ViewModel
        {
            get => (ProblemReportViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        /// <summary>
        /// Sorun listesine klavye odağı verir. Liste boşsa hiçbir şey yapmaz.
        /// Seçim yoksa ilk sorun seçilir.
        /// </summary>
        public void FocusProblems()
        {
            var viewModel = ViewModel;
            if (viewModel == null || !viewModel.HasProblems) return;

            if (viewModel.SelectedItem == null)
            {
                viewModel.SelectFirst();
            }

            ProblemList.Focus();
        }

        private void OnHighlightClick(object sender, RoutedEventArgs e)
        {
            ViewModel?.RequestHighlight();
        }

        private void OnProblemListPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            if (ViewModel == null || !ViewModel.CanHighlightSelected) return;

            ViewModel.RequestHighlight();
            e.Handled = true;
        }

        private void OnProblemListMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ViewModel == null || !ViewModel.CanHighlightSelected) return;

            ViewModel.RequestHighlight();
            e.Handled = true;
        }
    }
}
