<script setup>
import { ref } from 'vue';
import { findPath } from '@/helpers/tree.js';
import OtiumKategorieTag from '@/Otium/components/Shared/OtiumKategorieTag.vue';
import SimpleBreadcrumb from '@/components/SimpleBreadcrumb.vue';

const props = defineProps({
    options: Array,
    name: String,
    hideClear: Boolean,
    fullSize: Boolean,
    id: String,
    placeholder: {
        type: String,
        default: 'Kategorie',
    },
});

const emit = defineEmits(['change']);

const kategorie = defineModel();
const optionsTree = ref(convertToTreeSelectOptions(props.options));

function convertToTreeSelectOptions(options) {
    return options.map(treeMappingFunction);
}

function treeMappingFunction(element) {
    return {
        id: element.id,
        bezeichnung: element.bezeichnung,
        label: element.bezeichnung,
        icon: element.icon ?? undefined,
        color: element.cssColor ?? undefined,
        children: element.children ? convertToTreeSelectOptions(element.children) : undefined,
    };
}

const conditionalClose = (event, closePopover) => {
    if (!event.detail.value.children || event.detail.value.children.length === 0) {
        closePopover();
    }
};
</script>

<template>
    <UPopover>
        <UFieldGroup :class="{ 'w-full': fullSize }">
            <UButton
                :ui="{
                    base: 'flex justify-between w-full',
                }"
                color="neutral"
                size="lg"
                trailing-icon="i-lucide-chevron-down"
                variant="outline"
            >
                <template v-if="kategorie == null">Kategorie</template>
                <SimpleBreadcrumb v-else :model="findPath(options, kategorie.id)">
                    <template #item="{ item }">
                        <OtiumKategorieTag :value="item" minimal />
                    </template>
                </SimpleBreadcrumb>
            </UButton>
            <UButton
                v-if="!hideClear && kategorie != null"
                color="neutral"
                icon="i-lucide-x"
                label-key="id"
                size="lg"
                variant="outline"
                @click.stop="
                    () => {
                        kategorie = null;
                    }
                "
            />
        </UFieldGroup>
        <template #content="{ close }">
            <div class="p-2 min-w-64 max-h-60 overflow-y-auto">
                <UTree
                    v-model="kategorie"
                    :items="optionsTree"
                    color="neutral"
                    @select="(evt) => conditionalClose(evt, close)"
                >
                    <template #item-leading="{ item }">
                        <UIcon
                            v-if="item.icon"
                            :name="item.icon"
                            :style="{ color: item.color ?? 'inherit' }"
                        />
                    </template>
                </UTree>
            </div>
        </template>
    </UPopover>
</template>

<style scoped></style>
