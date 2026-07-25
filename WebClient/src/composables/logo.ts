import { computed } from 'vue';
import { useDark } from '@vueuse/core';
import wappenLight from '/vdaa/favicon.svg?url';
import wappenDark from '/vdaa/favicon-dark.svg?url';

export function useLogo() {
    const isDark = useDark();
    return computed(() => (isDark.value ? wappenDark : wappenLight));
}
