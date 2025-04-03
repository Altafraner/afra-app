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
    schuljahr: null,
    defaultDay: null,
    kategorien: null,
    personen: null,
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
    },
    async updateSchuljahr() {
      if (this.schuljahr) return;
      const termineGetter = mande("/api/schuljahr")
      try {
        const termine = await termineGetter.get();
        this.schuljahr = termine.schultage
        this.defaultDay = termine.standard
      } catch (error) {
        console.error("Error fetching schuljahr", error);
      }
    },
    async updatePersonen() {
      if (this.personen) return;
      const personenGetter = mande('/api/people')

      try {
        this.personen = await personenGetter.get();
      } catch (error) {
        console.error("Error fetching personen", error);
      }
    }
  }
})
