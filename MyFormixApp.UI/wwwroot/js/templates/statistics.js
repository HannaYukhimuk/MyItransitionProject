/**
 * Template Statistics Manager - handles template statistics page
 */
class TemplateStatisticsManager {
  constructor() {
    this.initCharts();
    this.initDateFilters();
  }

  /**
   * Initialize all charts
   */
  initCharts() {
    this.initSubmissionsChart();
    this.initQuestionCharts();
  }

  /**
   * Initialize submissions over time chart
   */
  initSubmissionsChart() {
    const ctx = document.getElementById('submissionsChart');
    if (!ctx) return;

    const chartData = {
      labels: JSON.parse(ctx.dataset.labels),
      datasets: [{
        label: 'Submissions',
        data: JSON.parse(ctx.dataset.values),
        backgroundColor: 'rgba(54, 162, 235, 0.2)',
        borderColor: 'rgba(54, 162, 235, 1)',
        borderWidth: 1,
        tension: 0.1,
        fill: true
      }]
    };

    new Chart(ctx, {
      type: 'line',
      data: chartData,
      options: {
        responsive: true,
        plugins: {
          tooltip: {
            mode: 'index',
            intersect: false
          }
        },
        scales: {
          y: {
            beginAtZero: true,
            precision: 0
          }
        }
      }
    });
  }

  /**
   * Initialize question statistics charts
   */
  initQuestionCharts() {
    document.querySelectorAll('.question-chart').forEach(chartElement => {
      const ctx = chartElement.getContext('2d');
      const chartData = {
        labels: JSON.parse(chartElement.dataset.labels),
        datasets: [{
          label: 'Responses',
          data: JSON.parse(chartElement.dataset.values),
          backgroundColor: this.generateChartColors(chartElement.dataset.labels.length),
          borderWidth: 1
        }]
      };

      new Chart(ctx, {
        type: chartElement.dataset.chartType || 'bar',
        data: chartData,
        options: {
          responsive: true,
          plugins: {
            legend: {
              display: false
            }
          },
          scales: {
            y: {
              beginAtZero: true,
              precision: 0
            }
          }
        }
      });
    });
  }

  /**
   * Generate colors for charts
   */
  generateChartColors(count) {
    const colors = [];
    const hueStep = 360 / count;
    
    for (let i = 0; i < count; i++) {
      colors.push(`hsla(${i * hueStep}, 70%, 60%, 0.7)`);
    }
    
    return colors;
  }

  /**
   * Initialize date range filters
   */
  initDateFilters() {
    const dateFilterForm = document.getElementById('dateFilterForm');
    if (!dateFilterForm) return;

    dateFilterForm.addEventListener('submit', (e) => {
      e.preventDefault();
      const formData = new FormData(dateFilterForm);
      const params = new URLSearchParams(formData).toString();
      
      window.location.href = `${window.location.pathname}?${params}`;
    });

    // Initialize date pickers
    const dateInputs = ['startDate', 'endDate'];
    dateInputs.forEach(id => {
      const input = document.getElementById(id);
      if (input) {
        new Datepicker(input, {
          format: 'yyyy-mm-dd',
          autohide: true
        });
      }
    });
  }
}

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', () => new TemplateStatisticsManager());