<script lang="ts" setup>
defineProps<{
    /** The Freistellungsantrag id. */
    antragId: string;
    /** The API base path for the current role, e.g. "/api/freistellung/sekretariat". */
    basePath: string;
}>();
</script>

<template>
    <UTooltip text="Als PDF exportieren">
        <UButton
            :href="`${basePath}/${antragId}.pdf`"
            external
            download
            icon="i-lucide-file-text"
            color="info"
            variant="ghost"
            size="sm"
            aria-label="Als PDF exportieren"
        />
    </UTooltip>
</template>
