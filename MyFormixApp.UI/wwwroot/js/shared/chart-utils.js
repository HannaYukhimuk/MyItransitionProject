/**
 * Chart Utilities - helper functions for working with charts
 */
class ChartUtils {
  /**
   * Initialize responsive charts container
   */
  static initResponsiveContainer() {
    document.querySelectorAll('.chart-container').forEach(container => {
      const aspectRatio = container.dataset.aspectRatio || 1.77;
      const height = container.offsetWidth / aspectRatio;
      container.style.height = `${height}px`;
      
      // Handle window resize
      window.addEventListener('resize', () => {
        const newHeight = container.offsetWidth / aspectRatio;
        container.style.height = `${newHeight}px`;
      });
    });
  }

  /**
   * Create a standard chart with common configuration
   */
  static createChart(canvasId, config) {
    const ctx = document.getElementById(canvasId);
    if (!ctx) return null;

    const defaultConfig = {
      type: 'bar',
      data: {
        labels: [],
        datasets: []
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: {
            position: 'top'
          },
          tooltip: {
            mode: 'index',
            intersect: false
          }
        },
        scales: {
          y: {
            beginAtZero: true
          }
        }
      }
    };

    const mergedConfig = this.deepMerge(defaultConfig, config);
    return new Chart(ctx, mergedConfig);
  }

  /**
   * Generate random colors for chart datasets
   */
  static generateColors(count, opacity = 1) {
    const colors = [];
    const hueStep = 360 / count;
    
    for (let i = 0; i < count; i++) {
      const hue = Math.floor(i * hueStep);
      colors.push(`hsla(${hue}, 70%, 60%, ${opacity})`);
    }
    
    return colors;
  }

  /**
   * Update chart data smoothly
   */
  static updateChart(chart, newData) {
    if (!chart || !newData) return;
    
    // Update labels
    if (newData.labels) {
      chart.data.labels = newData.labels;
    }
    
    // Update datasets
    if (newData.datasets) {
      newData.datasets.forEach((dataset, i) => {
        if (chart.data.datasets[i]) {
          chart.data.datasets[i].data = dataset.data;
          if (dataset.label) chart.data.datasets[i].label = dataset.label;
        } else {
          chart.data.datasets.push(dataset);
        }
      });
    }
    
    chart.update();
  }

  /**
   * Deep merge objects
   */
  static deepMerge(target, source) {
    const result = { ...target };
    
    for (const key in source) {
      if (source[key] instanceof Object && key in target) {
        result[key] = this.deepMerge(target[key], source[key]);
      } else {
        result[key] = source[key];
      }
    }
    
    return result;
  }

  /**
   * Export chart as PNG image
   */
  static exportChart(chart, fileName = 'chart') {
    if (!chart) return;
    
    const link = document.createElement('a');
    link.download = `${fileName}-${new Date().toISOString().slice(0, 10)}.png`;
    link.href = chart.toBase64Image();
    link.click();
  }

  /**
   * Initialize all charts with data attributes
   */
  static initChartsFromDataAttributes() {
    document.querySelectorAll('[data-chart]').forEach(element => {
      const config = {
        type: element.dataset.chartType || 'bar',
        data: {
          labels: JSON.parse(element.dataset.labels || '[]'),
          datasets: [{
            label: element.dataset.datasetLabel || 'Data',
            data: JSON.parse(element.dataset.values || '[]'),
            backgroundColor: this.generateColors(
              JSON.parse(element.dataset.values || '[]').length,
              element.dataset.backgroundOpacity || 0.7
            ),
            borderColor: this.generateColors(
              JSON.parse(element.dataset.values || '[]').length,
              1
            ),
            borderWidth: element.dataset.borderWidth || 1
          }]
        },
        options: {
          plugins: {
            title: {
              display: !!element.dataset.chartTitle,
              text: element.dataset.chartTitle || ''
            }
          }
        }
      };
      
      this.createChart(element.id, config);
    });
  }
}

// Initialize charts automatically if data attributes are present
document.addEventListener('DOMContentLoaded', () => {
  ChartUtils.initChartsFromDataAttributes();
  ChartUtils.initResponsiveContainer();
});

// Export for module systems if needed
if (typeof module !== 'undefined' && module.exports) {
  module.exports = ChartUtils;
}