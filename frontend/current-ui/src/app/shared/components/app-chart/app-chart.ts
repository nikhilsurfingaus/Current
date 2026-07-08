import {
  Component,
  ElementRef,
  OnDestroy,
  effect,
  input,
  viewChild,
} from '@angular/core';
import {
  Chart,
  ChartConfiguration,
  ChartData,
  ChartType,
  DoughnutController,
  ArcElement,
  BarController,
  BarElement,
  CategoryScale,
  LinearScale,
  LineController,
  LineElement,
  PointElement,
  Filler,
  Legend,
  Tooltip,
} from 'chart.js';

Chart.register(
  DoughnutController,
  ArcElement,
  BarController,
  BarElement,
  CategoryScale,
  LinearScale,
  LineController,
  LineElement,
  PointElement,
  Filler,
  Legend,
  Tooltip,
);

@Component({
  selector: 'app-chart',
  standalone: true,
  template: `
    <div class="app-chart" [style.height]="chartHeight()">
      <canvas #chartCanvas></canvas>
    </div>
  `,
  styles: `
    .app-chart {
      position: relative;
      width: 100%;
      height: 100%;
    }

    canvas {
      width: 100% !important;
      height: 100% !important;
    }
  `,
})
export class AppChartComponent implements OnDestroy {
  chartType = input.required<ChartType>();
  chartData = input.required<ChartData>();
  chartOptions = input<ChartConfiguration['options']>({});
  chartHeight = input('240px');

  private chartCanvas = viewChild.required<ElementRef<HTMLCanvasElement>>('chartCanvas');
  private chartInstance: Chart | null = null;

  constructor() {
    effect(() => {
      const type = this.chartType();
      const data = this.chartData();
      const options = this.chartOptions() ?? {};
      const canvasRef = this.chartCanvas();

      queueMicrotask(() => this.renderChart(type, data, options, canvasRef.nativeElement));
    });
  }

  ngOnDestroy(): void {
    this.destroyChart();
  }

  private renderChart(
    type: ChartType,
    data: ChartData,
    options: ChartConfiguration['options'],
    canvas: HTMLCanvasElement,
  ): void {
    this.destroyChart();

    this.chartInstance = new Chart(canvas, {
      type,
      data: structuredClone(data),
      options: {
        responsive: true,
        maintainAspectRatio: false,
        ...options,
      },
    });
  }

  private destroyChart(): void {
    if (!this.chartInstance) {
      return;
    }

    this.chartInstance.destroy();
    this.chartInstance = null;
  }
}
