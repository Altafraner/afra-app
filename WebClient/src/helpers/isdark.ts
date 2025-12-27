import { type Ref, ref } from 'vue';

export const isDark = (): Ref<boolean, boolean> => {
    const isDark = ref(window.matchMedia('(prefers-color-scheme: dark)').matches);

    window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', (e) => {
        isDark.value = e.matches;
    });

    return isDark;
};
