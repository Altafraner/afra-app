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
import { ref, computed, onMounted } from 'vue'
import { Radar } from 'vue-chartjs'
import { Chart, RadialLinearScale, PointElement, LineElement, Filler, Tooltip, Legend } from 'chart.js'

Chart.register(RadialLinearScale, PointElement, LineElement, Filler, Tooltip, Legend)

const RadarChart = Radar

const labels = ['Profundum 1', 'Profundum 2', 'Profundum 3', 'Profundum 4', 'Profundum 5', 'Profundum 6', 'Profundum 7', 'Profundum 8']

const datasetsRef = ref([
  {
    label: 'Dataset 1',
    data: [0, 0, 0, 0, 0, 0, 0, 0],
    backgroundColor: 'rgba(255, 99, 132, 0.2)',
    borderColor: 'rgba(255, 99, 132, 1)',
    borderWidth: 1,
    size: 1
  }
])

const currentIndex = ref(0)
const currentData = computed(() => ({
  labels,
  datasets: [datasetsRef.value[currentIndex.value]]
}))

function nextKriterium() {
  currentIndex.value = (currentIndex.value + 1) % datasetsRef.value.length
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

const loading = ref(true)
const error = ref(null)

onMounted(async () => {
  try {
    const res = await fetch('/api/profundum/bewertung/me', { credentials: 'same-origin' })
    if (!res.ok) throw new Error(`Fetch failed: ${res.status}`)
    const items = await res.json()

    const dataArr = new Array(labels.length).fill(0)
    let nextIndex = 0

    for (const it of items ?? []) {
      const grad = Number(it.grad ?? it.Grad ?? it.GradValue ?? 0)
      const kriterium = it.kriterium ?? it.Kriterium ?? it.kriteriumName ?? it.KriteriumName
      const kriteriumId = it.kriteriumId ?? it.KriteriumId ?? (kriterium && kriterium.id ? kriterium.id : undefined)
      let placed = false

      if (kriterium && typeof kriterium === 'object' && (kriterium.name || kriterium.Name)) {
        const name = (kriterium.name ?? kriterium.Name).toString()
        const idx = labels.findIndex(l => l.toLowerCase().includes(name.toLowerCase()) || name.toLowerCase().includes(l.toLowerCase()))
        if (idx >= 0) { dataArr[idx] = grad; placed = true }
      } else if (typeof kriterium === 'string') {
        const name = kriterium.toString()
        const idx = labels.findIndex(l => l.toLowerCase().includes(name.toLowerCase()) || name.toLowerCase().includes(l.toLowerCase()))
        if (idx >= 0) { dataArr[idx] = grad; placed = true }
      }

      if (!placed && typeof kriteriumId === 'number') {
        const idx = Number(kriteriumId) - 1
        if (idx >= 0 && idx < labels.length) { dataArr[idx] = grad; placed = true }
      }

      if (!placed) {
        while (nextIndex < labels.length && dataArr[nextIndex] !== 0) nextIndex++
        if (nextIndex < labels.length) { dataArr[nextIndex] = grad; nextIndex++ }
      }
    }

    datasetsRef.value[0].data = dataArr
    loading.value = false
  } catch (e) {
    error.value = e instanceof Error ? e.message : String(e)
    loading.value = false
    console.error('Why did you fail on me fam: ', error.value)
  }
})
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
