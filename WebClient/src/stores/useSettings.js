import {defineStore} from "pinia";
import {mande} from "mande";

export const useSettings = defineStore('settings', {
  state: () => ({
    blocks: [
      {
        startTime: new Date(0, 0, 0, 13, 30),
        endTime: new Date(0, 0, 0, 14, 45),
        id: 0
      },
      {
        startTime: new Date(0, 0, 0, 15, 0),
        endTime: new Date(0, 0, 0, 16, 15),
        id: 1
      }
    ],
    kategorien: null
  }),
  actions: {
    async updateKategorien() {
      if (this.kategorien) return;
      const kategorieGetter = mande('/api/Otium/kategorie');

      try {
        this.kategorien = await kategorieGetter.get();
      } catch (error) {
        console.error("Error fetching kategorien", error);
      }
    }
  }
})
