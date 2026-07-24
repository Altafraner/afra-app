<script lang="ts" setup>
import type { CalendarDateTime } from '@internationalized/date';

const modelValue = defineModel<CalendarDateTime | undefined>();
</script>

<template>
    <UPopover>
        <UInputDate ref="inputDate" v-model="modelValue">
            <template #trailing>
                <UButton
                    aria-label="Select a date"
                    class="px-0"
                    color="neutral"
                    icon="i-lucide-calendar"
                    size="sm"
                    variant="link"
                />
            </template>
        </UInputDate>

        <template #content>
            <div class="flex flex-col gap-2 p-2">
                <UCalendar v-model="modelValue" />
                <UInputTime v-model="modelValue" :hour-cycle="24" class="w-full" />
            </div>
        </template>
    </UPopover>
</template>
