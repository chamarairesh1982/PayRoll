import { Component, OnInit } from '@angular/core';
import { MessageService } from 'primeng/api';

@Component({
    selector: 'app-cost-analysis-page',
    templateUrl: './cost-analysis-page.component.html',
    styleUrls: ['./cost-analysis-page.component.scss'],
    providers: [MessageService]
})
export class CostAnalysisPageComponent implements OnInit {
    isLoading = false;

    // Intelligence Data Models
    costStructureData: any;
    trendAnalysisData: any;
    departmentCostData: any;
    chartOptions: any;
    lineOptions: any;
    barOptions: any;

    constructor(private messageService: MessageService) { }

    ngOnInit(): void {
        this.loadAnalysis();
    }

    loadAnalysis(): void {
        this.isLoading = true;
        this.initChartOptions();

        // Simulate API delay for analytical orchestration
        setTimeout(() => {
            this.prepareAnalyticalIntelligence();
            this.isLoading = false;
        }, 800);
    }

    private initChartOptions(): void {
        const documentStyle = getComputedStyle(document.documentElement);
        const textColor = documentStyle.getPropertyValue('--p-surface-700');
        const surfaceBorder = documentStyle.getPropertyValue('--p-surface-200');

        this.chartOptions = {
            maintainAspectRatio: false,
            aspectRatio: 0.8,
            plugins: {
                legend: {
                    labels: {
                        color: textColor,
                        font: { weight: '600', size: 12 }
                    }
                }
            },
            scales: {
                x: {
                    ticks: { color: textColor },
                    grid: { color: surfaceBorder, drawBorder: false }
                },
                y: {
                    ticks: { color: textColor },
                    grid: { color: surfaceBorder, drawBorder: false }
                }
            }
        };

        this.lineOptions = {
            ...this.chartOptions,
            maintainAspectRatio: false
        };

        this.barOptions = {
            ...this.chartOptions,
            indexAxis: 'y'
        };
    }

    private prepareAnalyticalIntelligence(): void {
        const documentStyle = getComputedStyle(document.documentElement);

        // Cost Structure: Gross vs Tax vs Net vs Statutory
        this.costStructureData = {
            labels: ['Net Disbursement', 'Institutional Tax', 'EPF Employer (12%)', 'ETF Employer (3%)', 'Other Benefits'],
            datasets: [
                {
                    data: [65, 12, 12, 3, 8],
                    backgroundColor: [
                        documentStyle.getPropertyValue('--p-primary-500'),
                        documentStyle.getPropertyValue('--p-secondary-500'),
                        documentStyle.getPropertyValue('--p-emerald-500'),
                        documentStyle.getPropertyValue('--p-amber-500'),
                        documentStyle.getPropertyValue('--p-indigo-500')
                    ]
                }
            ]
        };

        // Trend Analysis: 6 Month Trajectory
        this.trendAnalysisData = {
            labels: ['July', 'August', 'September', 'October', 'November', 'December'],
            datasets: [
                {
                    label: 'Total Personnel Cost',
                    data: [2.1, 2.3, 2.2, 2.8, 2.6, 3.1], // in millions
                    fill: true,
                    borderColor: documentStyle.getPropertyValue('--p-primary-500'),
                    tension: 0.4,
                    backgroundColor: 'rgba(79, 70, 229, 0.1)'
                }
            ]
        };

        // Departmental Cost Distribution
        this.departmentCostData = {
            labels: ['Engineering', 'Operations', 'Human Resources', 'Sales', 'Infrastructure'],
            datasets: [
                {
                    label: 'Current Period Cost (M LKR)',
                    data: [1.2, 0.8, 0.3, 0.5, 0.3],
                    backgroundColor: documentStyle.getPropertyValue('--p-primary-500'),
                    borderRadius: 8
                }
            ]
        };
    }
}
