import {defineStore} from "pinia";

export const useUser = defineStore('user', {
  state: () => ({
    loggedIn: true,
    user: {
      id: 0,
      vorname: 'Homer',
      nachname: 'Simpson',
      role: 'teacher'
    }
  })
})
