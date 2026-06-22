import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { DashboardService, DashboardSummary } from '../../services/dashboard.service';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DashboardComponent implements OnInit {
  summary?: DashboardSummary;
  loading = true;
  error = '';

  constructor(
    private dashboardService: DashboardService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.dashboardService.getSummary().subscribe({
      next: summary => {
        this.summary = summary;
        this.loading = false;
        this.cdr.markForCheck();
      },
      error: (err: Error) => {
        this.error = err.message || 'Failed to load dashboard';
        this.loading = false;
        this.cdr.markForCheck();
      }
    });
  }

  totalStatusCount(summary: DashboardSummary): number {
    return summary.statusCounts.todo + summary.statusCounts.inProgress + summary.statusCounts.done;
  }

  percent(value: number, total: number): number {
    return total === 0 ? 0 : Math.round((value / total) * 100);
  }

  pieStyle(summary: DashboardSummary): string {
    const total = this.totalStatusCount(summary);
    const todo = this.percent(summary.statusCounts.todo, total);
    const inProgress = todo + this.percent(summary.statusCounts.inProgress, total);

    return `conic-gradient(#60a5fa 0 ${todo}%, #f59e0b ${todo}% ${inProgress}%, #22c55e ${inProgress}% 100%)`;
  }

  typePieStyle(summary: DashboardSummary): string {
    const total = summary.typeCounts.task + summary.typeCounts.bug + summary.typeCounts.feature;
    const task = this.percent(summary.typeCounts.task, total);
    const bug = task + this.percent(summary.typeCounts.bug, total);

    return `conic-gradient(#2563eb 0 ${task}%, #ef4444 ${task}% ${bug}%, #10b981 ${bug}% 100%)`;
  }

  assignmentPieStyle(summary: DashboardSummary): string {
    const total = summary.assignmentCounts.assigned + summary.assignmentCounts.unassigned;
    const assigned = this.percent(summary.assignmentCounts.assigned, total);

    return `conic-gradient(#7c3aed 0 ${assigned}%, #94a3b8 ${assigned}% 100%)`;
  }
}
