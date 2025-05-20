// ==============================
// 📊 posetiva förändringar ifrån de två senaste bedömningarna för patient visas i stapeldiagram (PatientStatistics/Improvment)
// ==============================

// PatientStatistics/improvement
// === improvementChart.js ===
function renderImprovementChart(labels, previousData, currentData) {
    const ctx = document.getElementById('improvementChart');
    if (!ctx) return;

    new Chart(ctx, {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [
                {
                    label: 'Tidigare',
                    data: previousData,
                    backgroundColor: '#ffc107'
                },
                {
                    label: 'Nuvarande',
                    data: currentData,
                    backgroundColor: '#198754'
                }
            ]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: { position: 'bottom' },
                tooltip: {
                    mode: 'index',
                    intersect: false
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    max: 4,
                    ticks: { stepSize: 1 },
                    title: {
                        display: true,
                        text: 'Skattning (lägre = bättre)'
                    }
                }
            }
        }
    });
}

// PatientStatistics/improvement
// === Dölj sektion ===
function toggleDetailsPatient() {
    const section = document.getElementById('detailsSection');
    const button = document.getElementById('toggleButton');

    if (!section || !button) return;

    const isVisible = section.style.display === 'block';
    section.style.display = isVisible ? 'none' : 'block';
    button.innerText = isVisible ? '🔍 Visa detaljer per fråga' : '🔜 Dölj detaljer';
}

// ==============================
// 📊 posetiva frågesvar ifrån en bedömning för patient visas i en piechart (PatientStatistics/Singel)
// ==============================

// PatientStatistics/Single
// === piechart patient ===
function renderPatientSinglePieChart(labels, data) {
    const ctx = document.getElementById('resultPie');
    if (!ctx) return;

    new Chart(ctx, {
        type: 'pie',
        data: {
            labels: labels,
            datasets: [{
                data: data,
                backgroundColor: ['#198754', '#ffc107', '#dee2e6'],
                borderWidth: 1
            }]
        },
        options: {
            plugins: {
                legend: { position: 'bottom' },
                tooltip: {
                    callbacks: {
                        label: function (ctx) {
                            const label = ctx.label || '';
                            const value = ctx.parsed || 0;
                            return `${label}: ${value} frågor`;
                        }
                    }
                },
                datalabels: {
                    color: '#000',
                    font: {
                        weight: 'bold',
                        size: 16
                    },
                    formatter: (value, ctx) => {
                        const total = ctx.chart.data.datasets[0].data.reduce((a, b) => a + b, 0);
                        if (total === 0 || value === 0) return '';
                        const percent = Math.round((value / total) * 100);
                        return `${percent}%`;
                    }
                }
            }
        },
        plugins: [ChartDataLabels]
    });
}

// ==============================
// 📊 Autoscroll funktion till vårdgivare översiktvy (StaffResult)
// ==============================

// StaffResult/Index
// === Autoskroll vid svar/flagg-kommentar ===
function rememberScroll(id) {
    sessionStorage.setItem('scrollTo', id);
}

window.addEventListener('load', function () {
    const lastId = sessionStorage.getItem('scrollTo');
    if (lastId) {
        const el = document.getElementById(lastId);
        if (el) el.scrollIntoView({ behavior: 'smooth', block: 'center' });
        sessionStorage.removeItem('scrollTo');
    }
});

// ==============================
// 📊 Förändringar över tid negativ och posetiva ändringar visas i stapeldiagram (StaffChangeOverview)
// ==============================

let chartInstance = null; // Global referens till diagraminstansen

// StaffChangeOverview
/**
 * 🔄 Uppdaterar stapeldiagrammet baserat på valt filter (alla / förbättring / försämring)
 */
function updateChart(filterType) {
    // Steg 1: Klassificera varje datapunkt
    const rawTypes = rawPrevious.map((prev, i) => {
        const curr = rawCurrent[i];
        if (curr < prev) return "Förbättring";
        if (curr > prev) return "Försämring";
        return "Oförändrad";
    });

    // Steg 2: Filtrera index enligt valt filter
    const filteredIndexes = rawTypes
        .map((type, i) => {
            if (filterType === 'improvement' && type === "Förbättring") return i;
            if (filterType === 'deterioration' && type === "Försämring") return i;
            if (filterType === 'all') return i;
            return -1;
        })
        .filter(i => i !== -1);

    // Steg 3: Extrahera data för diagrammet
    const labels = filteredIndexes.map(i => rawLabels[i]);
    const previous = filteredIndexes.map(i => rawPrevious[i]);
    const current = filteredIndexes.map(i => rawCurrent[i]);
    const barColors = filteredIndexes.map(i =>
        rawCurrent[i] < rawPrevious[i] ? "#28a745" : "#dc3545"
    );

    // Steg 4: Uppdatera rubrik
    const chartTitle = document.getElementById("chartFilterLabel");
    if (chartTitle) {
        chartTitle.innerText =
            filterType === "all"
                ? "Visar alla resultat"
                : filterType === "improvement"
                    ? "Visar endast förbättringar"
                    : "Visar endast försämringar";
    }

    // Steg 5: Rensa befintligt diagram
    if (chartInstance) chartInstance.destroy();

    // Steg 6: Rita nytt diagram
    const ctx = document.getElementById('improvementChart');
    if (!ctx) return;

    chartInstance = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [
                {
                    label: 'Tidigare',
                    data: previous,
                    backgroundColor: '#ffc107'
                },
                {
                    label: 'Nuvarande',
                    data: current,
                    backgroundColor: barColors
                }
            ]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: { position: 'bottom' },
                tooltip: {
                    mode: 'index',
                    intersect: false
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    max: 4,
                    ticks: { stepSize: 1 },
                    title: {
                        display: true,
                        text: 'Skattning (lägre = bättre)'
                    }
                }
            }
        }
    });
}


// StaffChangeOverview
/**
 * 📂 Filtrerar både diagram och tabell samt återkopplingspaneler
 */
function filterData(type) {
    updateChart(type);
    filterTable(type);

    // Visa/dölj återkopplingspaneler
    const showImprovement = (type === 'all' || type === 'improvement');
    const showDeterioration = (type === 'all' || type === 'deterioration');

    const toggleDisplay = (id, show) => {
        const el = document.getElementById(id);
        if (el) el.style.display = show ? 'block' : 'none';
    };

    toggleDisplay("feedbackImprovement", showImprovement);
    toggleDisplay("feedbackImprovementNote", showImprovement);
    toggleDisplay("feedbackDeterioration", showDeterioration);
    toggleDisplay("feedbackDeteriorationNote", showDeterioration);
}

// StaffChangeOverview
/**
 * 📋 Filtrerar tabellrader visuellt efter typ
 */
function filterTable(type) {
    const improvementRows = document.querySelectorAll('.table-success');
    const deteriorationRows = document.querySelectorAll('.table-danger');

    improvementRows.forEach(row =>
        row.style.display = type === 'deterioration' ? 'none' : 'table-row'
    );
    deteriorationRows.forEach(row =>
        row.style.display = type === 'improvement' ? 'none' : 'table-row'
    );
}

// StaffChangeOverview
/**
 * 👁 Växlar visning av den detaljerade frågetabellen
 */
function toggleDetailsStaff() {
    const section = document.getElementById('detailsSection');
    const button = document.getElementById('toggleButton');
    const visible = section && section.style.display === 'block';

    if (section) section.style.display = visible ? 'none' : 'block';
    if (button) button.textContent = visible ? '🔍 Visa detaljer' : '🔍 Dölj detaljer';
}

// StaffChangeOverview
/** 
 * 🚀 Initierar vyn vid sidladdning
 */
document.addEventListener('DOMContentLoaded', function () {
    const chartEl = document.getElementById('improvementChart');
    if (chartEl) {
        const toggleBtn = document.getElementById('toggleButton');
        const detailSection = document.getElementById('detailsSection');

        if (toggleBtn && detailSection) {
            detailSection.style.display = 'none';
            toggleBtn.textContent = '🔍 Visa detaljer';
        }

        updateChart('all');
        filterTable('all');
    }
});

// ==============================
// 📊 Jämnförelse mellan vårdgivare och patient i en bedömning visas i en piechart (Comparison)
// ==============================


// StaffStatistics/Comparison
// === Chart: Fördelning av skillnader mellan svar (utan 'Obesvarad') ===
function renderStaffComparisonPie(labels, values) {
    const ctx = document.getElementById('comparisonPie');
    if (!ctx) return;

    // Registrera plugin för att visa procent
    Chart.register(ChartDataLabels);

    // Filtrera bort "Obesvarad" (eller vad som motsvarar det)
    const filtered = labels
        .map((label, index) => ({ label, value: values[index] }))
        .filter(entry => !entry.label.includes("Obesvarad") && entry.value > 0);

    const filteredLabels = filtered.map(entry => entry.label);
    const filteredValues = filtered.map(entry => entry.value);

    const filteredColors = ['#dc3545', '#ffc107', '#198754']; // Anpassa färger om fler kategorier

    new Chart(ctx, {
        type: 'pie',
        data: {
            labels: filteredLabels,
            datasets: [{
                data: filteredValues,
                backgroundColor: filteredColors
            }]
        },
        options: {
            plugins: {
                legend: { position: 'bottom' },
                tooltip: {
                    callbacks: {
                        label: function (ctx) {
                            const label = ctx.label || '';
                            const value = ctx.parsed || 0;
                            return `${label}: ${value} frågor`;
                        }
                    }
                },
                datalabels: {
                    color: '#000',
                    font: {
                        weight: 'bold',
                        size: 14
                    },
                    formatter: (value, ctx) => {
                        const total = ctx.chart.data.datasets[0].data.reduce((a, b) => a + b, 0);
                        if (total === 0 || value === 0) return '';
                        const percent = Math.round((value / total) * 100);
                        return `${percent}%`;
                    }
                }
            }
        },
        plugins: [ChartDataLabels]
    });
}



// StaffStatistics/Comparison
// === Toggle: Visa/dölj frågetabell ===
function toggleComparisonTable() {
    const section = document.getElementById("questionTableSection");
    if (!section) return;
    section.style.display = section.style.display === "none" ? "block" : "none";
}

// StaffStatistics/Comparison
// === Filter: Tillämpa filter baserat på val i dropdown ===
function applyComparisonFilters() {
    const filter = document.getElementById("filterSelector")?.value;
    const rows = document.querySelectorAll("#comparisonTable tbody tr");

    rows.forEach(row => {
        const type = row.dataset.type;
        const flagged = row.dataset.flagged === "true";
        const commented = row.dataset.commented === "true";

        let show = false;
        if (filter === "all") show = true;
        else if (filter === "flagged") show = flagged;
        else if (filter === "commented") show = commented;
        else show = type === filter;

        row.style.display = show ? "table-row" : "none";
    });
}

// StaffStatistics/PatientAnswerSummary
// === Piechart: Sammanställning av patientens egna svar ===
function renderPatientAnswerSummaryPie(labels, data) {
    const ctx = document.getElementById('patientAnswerPie');
    if (!ctx) return;

    Chart.register(ChartDataLabels);

    new Chart(ctx, {
        type: 'pie',
        data: {
            labels: labels,
            datasets: [{
                data: data,
                backgroundColor: [
                    '#198754', // 0 - grön
                    '#74c37e', // 1 - ljusgrön
                    '#ffc107', // 2 - gul
                    '#fd7e14', // 3 - orange
                    '#dc3545', // 4 - röd
                    '#dee2e6'  // null - grå
                ]
            }]
        },
        options: {
            plugins: {
                legend: { position: 'bottom' },
                tooltip: {
                    callbacks: {
                        label: function (ctx) {
                            const label = ctx.label || '';
                            const value = ctx.parsed || 0;
                            return `${label}: ${value} frågor`;
                        }
                    }
                },
                datalabels: {
                    color: '#000',
                    font: { weight: 'bold', size: 14 },
                    formatter: (value, ctx) => {
                        const total = ctx.chart.data.datasets[0].data.reduce((a, b) => a + b, 0);
                        if (total === 0 || value === 0) return '';
                        const percent = Math.round((value / total) * 100);
                        return `${percent}%`;
                    }
                }
            }
        },
        plugins: [ChartDataLabels]
    });
}

// ==============================
// 📋 PatientAnswerSummary – Visa/dölj och filtrera frågetabell
// ==============================

function togglePatientAnswerTable() {
    const section = document.getElementById("patientAnswerTableSection");
    if (!section) return;
    section.style.display = section.style.display === "none" ? "block" : "none";
}

function applyPatientAnswerFilter() {
    const filter = document.getElementById("patientAnswerFilter")?.value;
    const rows = document.querySelectorAll("#patientAnswerTable tbody tr");

    rows.forEach(row => {
        const svar = row.dataset.svar;
        let show = false;

        if (filter === "all") show = true;
        else if (filter === "null") show = svar === "null";
        else show = svar === filter;

        row.style.display = show ? "table-row" : "none";
    });
}

// ==============================
// 📊 Patient – Jämför förbättring över tid
// ==============================

function validateDates() {
    const first = document.getElementById("firstSelect");
    const second = document.getElementById("secondSelect");

    const firstDate = new Date(first.options[first.selectedIndex].getAttribute("data-date"));
    const secondDate = new Date(second.options[second.selectedIndex].getAttribute("data-date"));

    const errorDiv = document.getElementById("dateError");

    if (firstDate >= secondDate) {
        errorDiv.style.display = "block";
        return false;
    }

    errorDiv.style.display = "none";
    return true;
}

// ==============================
// 📊 StaffStatistics – Validera datum för jämförelse (bedömning 1 måste vara äldre)
// ==============================

function validateDates() {
    const first = document.getElementById("firstSelect");
    const second = document.getElementById("secondSelect");

    const firstDate = new Date(first.options[first.selectedIndex].getAttribute("data-date"));
    const secondDate = new Date(second.options[second.selectedIndex].getAttribute("data-date"));

    const errorDiv = document.getElementById("dateError");

    if (firstDate >= secondDate) {
        errorDiv.style.display = "block";
        return false;
    }

    errorDiv.style.display = "none";
    return true;
}

// ==============================
// 📊 StaffAssessment söka i patient lista)
// ==============================
function clearSearch() {
    const searchInput = document.querySelector('input[name="search"]');
    if (searchInput) {
        searchInput.value = '';
    }
    window.location.href = searchInput?.closest("form")?.getAttribute("action") || window.location.href;
}
