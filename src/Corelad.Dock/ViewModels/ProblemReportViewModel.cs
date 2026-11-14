using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Corelad.Dock.Diagnostics;
using Corelad.Geometry.Analysis;
using Corelad.Geometry.Diagnostics;

namespace Corelad.Dock.ViewModels
{
    /// <summary>
    /// Sorun raporu panelinin sunum modeli. <see cref="SmartDivideAnalysis"/> / <see cref="Problem"/>
    /// verisini okur, Türkçe sunuma çevirir ve seçilen sorun için kontur vurgu isteği yayınlar.
    ///
    /// Sınırlar:
    /// - Geometri algoritması veya COM çağrısı içermez (AGENTS.md §2).
    /// - UI thread'inde kullanılır; <see cref="Load(SmartDivideAnalysis)"/> arka planda çağrılmaz.
    /// - Vurgu isteğini tüketen katman <see cref="HighlightRequested"/> olayına veya
    ///   <see cref="HighlightSink"/> arayüzüne bağlanır; bu görevde uygulama yoktur.
    /// </summary>
    public sealed class ProblemReportViewModel : INotifyPropertyChanged
    {
        private IReadOnlyList<ProblemReportEntry> _items = Array.Empty<ProblemReportEntry>();
        private ProblemReportEntry _selectedItem;
        private bool _suppressHighlightDispatch;
        private bool _isLoaded;
        private int _errorCount;
        private int _warningCount;
        private int _infoCount;

        public ProblemReportViewModel()
        {
        }

        public ProblemReportViewModel(SmartDivideAnalysis analysis)
        {
            Load(analysis);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>Sorun seçildiğinde veya vurgu yeniden istendiğinde tetiklenir.</summary>
        public event EventHandler<ContourHighlightRequestedEventArgs> HighlightRequested;

        /// <summary>
        /// Olay yerine arayüzle bağlanmak isteyen tüketiciler için opsiyonel kanal.
        /// <see cref="HighlightRequested"/> ile aynı anda çağrılır.
        /// </summary>
        public IContourHighlightRequestSink HighlightSink { get; set; }

        public IReadOnlyList<ProblemReportEntry> Items => _items;

        public int TotalCount => _items.Count;

        public int ErrorCount => _errorCount;

        public int WarningCount => _warningCount;

        public int InfoCount => _infoCount;

        /// <summary>Bir analiz yüklenip yüklenmediği. Boş rapor ile "henüz rapor yok" ayrımını sağlar.</summary>
        public bool IsLoaded => _isLoaded;

        public bool HasProblems => _items.Count > 0;

        public bool HasErrors => _errorCount > 0;

        public bool HasWarnings => _warningCount > 0;

        public bool HasInfo => _infoCount > 0;

        public string ListHeading => HasProblems
            ? string.Format(CultureInfo.CurrentCulture, "Sorunlar ({0})", TotalCount)
            : "Sorunlar";

        public string SummaryText
        {
            get
            {
                if (!_isLoaded) return "Rapor bekleniyor";
                if (!HasProblems) return "Sorun bulunmadı";

                var parts = new List<string>(3);
                if (_errorCount > 0) parts.Add(FormatNumber(_errorCount) + " hata");
                if (_warningCount > 0) parts.Add(FormatNumber(_warningCount) + " uyarı");
                if (_infoCount > 0) parts.Add(FormatNumber(_infoCount) + " bilgi");
                return string.Join(", ", parts);
            }
        }

        public string SummaryAccessibleText
        {
            get
            {
                if (!_isLoaded) return "Henüz rapor yok. Smart Divide çalıştırıldığında sorunlar burada listelenir.";
                if (!HasProblems) return "Sorun bulunmadı. Analiz hata veya uyarı üretmedi.";

                return string.Format(
                    CultureInfo.CurrentCulture,
                    "Toplam {0} sorun: {1}.",
                    TotalCount,
                    SummaryText);
            }
        }

        public string ErrorStatText => FormatCount(_errorCount, "hata");

        public string WarningStatText => FormatCount(_warningCount, "uyarı");

        public string InfoStatText => FormatCount(_infoCount, "bilgi");

        public string EmptyStateGlyph => _isLoaded ? "✓" : "…";

        public string EmptyStateTitle => _isLoaded ? "Sorun bulunmadı" : "Henüz rapor yok";

        public string EmptyStateMessage => _isLoaded
            ? "Smart Divide analizi hata veya uyarı üretmedi. Seçili çizim mantıksal harflere ayrılmaya hazır."
            : "Smart Divide çalıştırıldığında hata ve uyarılar burada listelenir.";

        public string EmptyStateFootnote =>
            "Bu panel yalnızca raporu gösterir; kaynak çizimi değiştirmez veya kaydetmez.";

        public ProblemReportEntry SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (ReferenceEquals(_selectedItem, value)) return;

                _selectedItem = value;
                OnPropertyChanged(nameof(SelectedItem));
                OnPropertyChanged(nameof(HasSelection));
                OnPropertyChanged(nameof(CanHighlightSelected));
                OnPropertyChanged(nameof(HighlightHint));

                if (!_suppressHighlightDispatch)
                {
                    RequestHighlight(value);
                }
            }
        }

        public bool HasSelection => _selectedItem != null;

        public bool CanHighlightSelected => _selectedItem != null && _selectedItem.HasContourTarget;

        public string HighlightHint
        {
            get
            {
                if (!_isLoaded || !HasProblems) return string.Empty;

                if (_selectedItem == null)
                {
                    return "Konturu CorelDRAW'da vurgulamak için bir sorun seçin.";
                }

                if (!_selectedItem.HasContourTarget)
                {
                    return "Seçili sorun bir kontura bağlı değil; vurgulanacak kontur yok.";
                }

                return string.Format(
                    CultureInfo.CurrentCulture,
                    "Kontur {0} için vurgu isteği iletildi. Enter ile yeniden isteyebilirsiniz.",
                    (_selectedItem.HighlightContourIndex ?? 0) + 1);
            }
        }

        /// <summary>Analiz sonucunu yükler. Önceki seçim temizlenir; vurgu isteği yayınlanmaz.</summary>
        public void Load(SmartDivideAnalysis analysis)
        {
            if (analysis == null) throw new ArgumentNullException(nameof(analysis));
            Load(analysis.Problems);
        }

        /// <summary>Sorun listesini yükler. Hatalar önce, sonra uyarılar ve bilgiler gelir.</summary>
        public void Load(IReadOnlyList<Problem> problems)
        {
            if (problems == null) throw new ArgumentNullException(nameof(problems));

            var ordered = problems
                .Where(problem => problem != null)
                .OrderByDescending(problem => (int)problem.Severity)
                .Select(problem => new ProblemReportEntry(problem))
                .ToList();

            Apply(ordered, isLoaded: true);
        }

        /// <summary>Paneli "henüz rapor yok" durumuna döndürür.</summary>
        public void Clear()
        {
            Apply(Array.Empty<ProblemReportEntry>(), isLoaded: false);
        }

        /// <summary>İlk sorunu seçer (klavye odağı için). Liste boşsa hiçbir şey yapmaz.</summary>
        public void SelectFirst()
        {
            if (_items.Count > 0)
            {
                SelectedItem = _items[0];
            }
        }

        /// <summary>Seçili sorun için kontur vurgu isteğini yeniden yayınlar.</summary>
        public bool RequestHighlight()
        {
            return RequestHighlight(_selectedItem);
        }

        /// <summary>
        /// Belirtilen sorun için kontur vurgu isteği yayınlar. Sorun yoksa veya sorun bir
        /// kontura bağlı değilse <c>false</c> döner ve hiçbir istek yayınlanmaz.
        /// </summary>
        public bool RequestHighlight(ProblemReportEntry entry)
        {
            if (entry == null) return false;

            var request = new ContourHighlightRequest(entry.Source);
            if (!request.HasTarget) return false;

            HighlightRequested?.Invoke(this, new ContourHighlightRequestedEventArgs(request));
            HighlightSink?.HandleHighlightRequest(request);
            return true;
        }

        private void Apply(IReadOnlyList<ProblemReportEntry> ordered, bool isLoaded)
        {
            _suppressHighlightDispatch = true;
            try
            {
                SelectedItem = null;
                _items = ordered;
                _isLoaded = isLoaded;
                _errorCount = ordered.Count(entry => entry.IsError);
                _warningCount = ordered.Count(entry => entry.IsWarning);
                _infoCount = ordered.Count(entry => entry.IsInfo);
            }
            finally
            {
                _suppressHighlightDispatch = false;
            }

            OnPropertyChanged(nameof(Items));
            OnPropertyChanged(nameof(TotalCount));
            OnPropertyChanged(nameof(ErrorCount));
            OnPropertyChanged(nameof(WarningCount));
            OnPropertyChanged(nameof(InfoCount));
            OnPropertyChanged(nameof(IsLoaded));
            OnPropertyChanged(nameof(HasProblems));
            OnPropertyChanged(nameof(HasErrors));
            OnPropertyChanged(nameof(HasWarnings));
            OnPropertyChanged(nameof(HasInfo));
            OnPropertyChanged(nameof(ListHeading));
            OnPropertyChanged(nameof(SummaryText));
            OnPropertyChanged(nameof(SummaryAccessibleText));
            OnPropertyChanged(nameof(ErrorStatText));
            OnPropertyChanged(nameof(WarningStatText));
            OnPropertyChanged(nameof(InfoStatText));
            OnPropertyChanged(nameof(EmptyStateGlyph));
            OnPropertyChanged(nameof(EmptyStateTitle));
            OnPropertyChanged(nameof(EmptyStateMessage));
            OnPropertyChanged(nameof(EmptyStateFootnote));
            OnPropertyChanged(nameof(SelectedItem));
            OnPropertyChanged(nameof(HasSelection));
            OnPropertyChanged(nameof(CanHighlightSelected));
            OnPropertyChanged(nameof(HighlightHint));
        }

        private static string FormatCount(int count, string label)
        {
            return FormatNumber(count) + " " + label;
        }

        private static string FormatNumber(int value)
        {
            return value.ToString(CultureInfo.CurrentCulture);
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
