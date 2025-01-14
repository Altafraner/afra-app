<script setup>

import {ref} from "vue";

defineProps(['classes'])
const emit = defineEmits(['user-created'])

const form = ref(false)
const dialog = ref(false)
const loading = ref(false)
const first_name = ref(null)
const last_name = ref(null)
const class_id = ref(null)
const email = ref(null)

function onCancel(){
  if(loading.value) return
  first_name.value = null
  last_name.value = null
  class_id.value = null
  email.value=null
  dialog.value=false
}

async function onSubmit(){
  loading.value=true
  const body = {
    firstName: first_name.value,
    lastName: last_name.value,
    email: email.value,
    classId: class_id.value
  }

  const result = await fetch("http://localhost:5043/api/Person", {
    method:"POST",
    body: JSON.stringify(body),
    headers: {
      "content-type": "application/json"
    }
  })

  if (!result.ok){
    alert("Der Nutzer konnte *nicht* erstellt werden.")
    return
  }

  first_name.value = null
  last_name.value = null
  class_id.value = null
  email.value=null
  loading.value=false
  dialog.value=false

  emit("user-created")
}

</script>

<template>
  <v-dialog max-width="500" v-model="dialog" :persistent="loading">
    <template v-slot:activator="{ props: activatorProps }">
      <v-btn
        prepend-icon="mdi-account-plus"
        variant="elevated"
        color="primary"
        class="text-none"
        v-bind="activatorProps">
        Nutzer hinzufügen
      </v-btn>
    </template>
    <template v-slot:default>
      <v-form @submit.prevent="onSubmit" v-model="form">
      <v-card>
        <v-card-title>Nutzer hinzufügen</v-card-title>
        <v-divider></v-divider>
        <v-card-text>
          <p>Persönliche Informationen</p>
          <div class="d-flex ga-4">
            <v-text-field v-model="first_name" label="Vorname" required density="comfortable"></v-text-field>
            <v-text-field v-model="last_name" label="Nachname" required density="comfortable" s></v-text-field>
          </div>
          <v-text-field v-model="email" label="E-Mail-Adresse" required density="comfortable" type="email" validate-on="input lazy"></v-text-field>
          <p>Organisatorische Informationen (optional)</p>
          <v-select v-model="class_id" :items="classes" label="Klasse *" density="comfortable"></v-select>
          <small class="text-caption text-medium-emphasis">* Wird keine Klasse angegeben, wird die Person nicht als Schüler:in erkannt.</small>
        </v-card-text>
        <v-divider />
        <v-card-actions>
          <v-btn class="text-none" @click="onCancel">Abbrechen</v-btn>
          <v-btn class="text-none" type="submit" :loading="loading">Erstellen</v-btn>
        </v-card-actions>
      </v-card>
      </v-form>
    </template>
  </v-dialog>
</template>

<style scoped>

</style>
