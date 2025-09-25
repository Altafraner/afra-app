<template>
  <div class="bewertung-bg">
    <div class="bewertung-content">
      <RadarChart :data="currentData" :options="options" class="chart" />
      <button class="next-arrow" @click="nextKriterium" aria-label="Next">
        <svg width="40" height="40" viewBox="0 0 40 40">
          <circle cx="20" cy="20" r="18" fill="none" stroke="currentColor" stroke-width="2"/>
          <polyline points="15,12 25,20 15,28" fill="none" stroke="currentColor" stroke-width="3" stroke-linecap="round" stroke-linejoin="round"/>
        </svg>
      </button>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import { Radar } from 'vue-chartjs'
import { Chart, RadialLinearScale, PointElement, LineElement, Filler, Tooltip, Legend, Ticks } from 'chart.js'

Chart.register(RadialLinearScale, PointElement, LineElement, Filler, Tooltip, Legend)

const RadarChart = Radar

const labels = ['Profundum 1', 'Profundum 2', 'Profundum 3', 'Profundum 4', 'Profundum 5', 'Profundum 6', 'Profundum 7', 'Profundum 8']

const datasets = [
  {
    label: 'Dataset 1',
    data: [3, 4, 7, 2, 2, 3, 2, 2],
    backgroundColor: 'rgba(255, 99, 132, 0.2)',
    borderColor: 'rgba(255, 99, 132, 1)',
    borderWidth: 1,
    size: 1
  },
  {
    label: 'Dataset 2',
    data: [5, 4, 3, 2, 7, 6, 8, 7],
    backgroundColor: 'rgba(54, 162, 235, 0.2)',
    borderColor: 'rgba(54, 162, 235, 1)',
    borderWidth: 1,
    size: 1
  },
  {
    label: 'Dataset 3',
    data: [2, 6, 5, 4, 3, 7, 2, 5],
    backgroundColor: 'rgba(255, 206, 86, 0.2)',
    borderColor: 'rgba(255, 206, 86, 1)',
    borderWidth: 1,
    size: 1
  }
]

const currentIndex = ref(0)
const currentData = computed(() => ({
  labels,
  datasets: [datasets[currentIndex.value]]
}))

function nextKriterium() {
  currentIndex.value = (currentIndex.value + 1) % datasets.length
}

const isDark = window.matchMedia('(prefers-color-scheme: dark)').matches

const options = {
  scales: {
    r: {
      angleLines: {
        color: isDark ? 'rgba(255,255,255,0.3)' : 'rgba(255,255,255,0.6)'
      },
      grid: {
        color: isDark ? 'rgba(255,255,255,0.15)' : 'rgba(255,255,255,0.3)'
      },
      suggestedMin: 0,
      suggestedMax: 8,
      pointLabels: { font: { size: 16, color: isDark ? '#eee' : '#333' } },
      ticks: { display: false }
    }
  },
  plugins: {
    legend: { display: false }
  }
}


</script>

<style scoped>

.bewertung-content {
  position: relative;
  z-index: 1;
  display: flex;
  justify-content: space-around;
  align-items: center;
  width: 80vw;
  max-width: 900px;
}

.chart {
  width: 35vw;
  height: 35vw;
  max-width: 800px;
  max-height: 800px;
  background: rgba(255, 255, 255, 0.85);
  border-radius: 30px;
  box-shadow: 0 2px 16px rgba(224, 221, 221, 0.04);
  padding: 1vw;
}

.next-arrow {
  background: none;
  border: none;
  cursor: pointer;
  margin-left: 2vw;
  align-self: center;
  color: #3b82f6;
  transition: color 0.2s;
}
.next-arrow:hover {
  color: #2563eb;
}
@media (prefers-color-scheme: dark) {
  .chart {
    background: rgba(32, 32, 32, 0.85);
    box-shadow: 0 5px 16px rgba(0, 0, 0, 0.6);
  }
  .next-arrow {
    color: #60a5fa;
  }
  .next-arrow:hover {
    color: #38bdf8;
  }
}

</style>
