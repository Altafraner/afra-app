import {defineStore} from "pinia";

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
        ]
    })
})
