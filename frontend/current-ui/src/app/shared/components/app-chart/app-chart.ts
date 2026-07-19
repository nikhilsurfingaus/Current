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
    <div class="app-chart" [style.height]="chartHeight()" #chartHost>
      <canvas #chartCanvas></canvas>
    </div>
  `,
  styles: `
    .app-chart {
      position: relative;
      width: 100%;
      min-width: 0;
      overflow: hidden;
    }

    canvas {
      display: block;
    }
  `,
})
export class AppChartComponent implements OnDestroy {
  chartType = input.required<ChartType>();
  chartData = input.required<ChartData>();
  chartOptions = input<ChartConfiguration['options']>({});
  chartHeight = input('240px');

  private chartHost = viewChild.required<ElementRef<HTMLDivElement>>('chartHost');
  private chartCanvas = viewChild.required<ElementRef<HTMLCanvasElement>>('chartCanvas');
  private chartInstance: Chart | null = null;
  private resizeObserver: ResizeObserver | null = null;

  constructor() {
    effect((onCleanup) => {
      const type = this.chartType();
      const data = this.chartData();
      const options = this.chartOptions() ?? {};
      const canvas = this.chartCanvas().nativeElement;
      const host = this.chartHost().nativeElement;

      queueMicrotask(() => {
        this.renderChart(type, data, options, canvas);
        this.observeHostSize(host);
      });

      onCleanup(() => {
        this.resizeObserver?.disconnect();
        this.resizeObserver = null;
      });
    });
  }

  ngOnDestroy(): void {
    this.resizeObserver?.disconnect();
    this.resizeObserver = null;
    this.destroyChart();
  }

  private observeHostSize(host: HTMLDivElement): void {
    this.resizeObserver?.disconnect();

    this.resizeObserver = new ResizeObserver(() => {
      this.chartInstance?.resize();
    });
    this.resizeObserver.observe(host);
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

    requestAnimationFrame(() => this.chartInstance?.resize());
  }

  private destroyChart(): void {
    if (!this.chartInstance) {
      return;
    }

    this.chartInstance.destroy();
    this.chartInstance = null;
  }
}
