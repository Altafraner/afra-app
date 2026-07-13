<script setup>
import { computed, ref } from 'vue';
import { findPath } from '@/helpers/tree.js';
import OtiumKategorieTag from '@/Otium/components/Shared/OtiumKategorieTag.vue';
import SimpleBreadcrumb from '@/components/SimpleBreadcrumb.vue';
import { useFormField } from '@nuxt/ui/composables';

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
    color: {
        type: String,
        default: 'secondary',
    },
});

const emit = defineEmits(['change']);

const formField = useFormField();

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
    formField.emitFormChange();
    formField.emitFormInput();
    formField.emitFormFocus();
    if (!event.detail.value.children || event.detail.value.children.length === 0) {
        closePopover();
    }
};

const color = computed(() => formField.color.value ?? props.color);
</script>

<template>
    <UPopover>
        <UFieldGroup :class="{ 'w-full': fullSize }">
            <UButton
                :ui="{
                    base: 'flex justify-between w-full',
                }"
                trailing-icon="i-lucide-chevron-down"
                variant="outline"
                :color="color"
                v-bind="$attrs"
                @click="formField.emitFormInput"
            >
                <span v-if="kategorie == null" class="text-dimmed">{{ placeholder }}</span>
                <SimpleBreadcrumb v-else :model="findPath(options, kategorie.id)">
                    <template #item="{ item }">
                        <OtiumKategorieTag :value="item" minimal />
                    </template>
                </SimpleBreadcrumb>
            </UButton>
            <UButton
                v-if="!hideClear && kategorie != null"
                icon="i-lucide-x"
                label-key="id"
                variant="outline"
                @click.stop="
                    () => {
                        kategorie = null;
                    }
                "
                :color="color"
                @click="formField.emitFormChange"
            />
        </UFieldGroup>
        <template #content="{ close }">
            <div class="p-2 min-w-64 max-h-60 overflow-y-auto">
                <UTree
                    v-model="kategorie"
                    :items="optionsTree"
                    color="neutral"
                    :id="formField.id"
                    :name="formField.name"
                    @select="(evt) => conditionalClose(evt, close)"
                    @blur="formField.emitFormBlur"
                    @change="formField.emitFormChange"
                    @focus="formField.emitFormFocus"
                    @input="formField.emitFormInput"
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
