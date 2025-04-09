<script setup>
import {Form} from '@primevue/forms';
import {Button, FloatLabel, InputText, Message, Password, Select, useToast} from "primevue";
import {ref} from "vue";
import {mande} from "mande";
import {useUser} from "@/stores/useUser.js";

const loading = ref(false)
const user = useUser()
const toast = useToast()

const options = [
  {label: 'Lehrer:in', value: 'Tutor'},
  {label: 'Schüler:in', value: 'Student'},
]

const submit = async (evt) => {
  if (loading.value || !evt.valid) return;
  const username = evt.states['username'].value;
  const password = evt.states['password'].value;
  const roll = evt.states['roll'].value;
  if (!(username && password)) return;
  loading.value = true;
  try {
    await mande('/api/user/login').post({
      username: username,
      password: password,
      roll: roll
    }, {responseAs: "response"});
    await user.update();
  } catch (error) {
    if (error.response.status === 401) {
      toast.add({
        severity: "error",
        summary: "Fehler",
        detail: "Fehlerhafte Anmeldedaten",
        life: 5000
      });
    } else {
      toast.add({
        severity: "error",
        summary: "Fehler",
        detail: "Ein unbekannter Fehler ist aufgetreten",
        life: 5000
      });
    }
  } finally {
    loading.value = false;
  }
}

function resolve({values}) {
  const errors = {}

  if (!values.username || values.username.trim() === "")
    errors.username = [{message: "Es muss ein Nutzername angegeben werden!"}]

  if (!values.password || values.password.trim() === "")
    errors.password = [{message: "Es muss ein Passwort angegeben werden!"}]

  if (!values.roll)
    errors.roll = [{message: "Es muss ein Rolle angegeben werden!"}]

  return {values, errors}
}

</script>

<template>
  <Form v-slot="$form" :resolver="resolve" class="flex flex-col gap-6 mt-8" @submit="submit">
    <div class="w-full">
      <FloatLabel variant="on">
        <InputText id="username" fluid name="username" type="text"/>
        <label for="username">Nutzername</label>
      </FloatLabel>
      <Message v-if="$form.username?.invalid" severity="error" size="small" variant="simple">
        {{ $form.username.error.message }}
      </Message>
    </div>
    <div class="w-full">
      <FloatLabel variant="on">
        <Password :feedback="false" fluid input-id="password" name="password" toggle-mask/>
        <label for="password">Passwort</label>
      </FloatLabel>
      <Message v-if="$form.password?.invalid" severity="error" size="small" variant="simple">
        {{ $form.password.error.message }}
      </Message>
    </div>
    <div class="w-full">
      <FloatLabel variant="on">
        <Select id="roll" :options="options" fluid name="roll" option-label="label"
                option-value="value"/>
        <label for="roll">Rolle</label>
      </FloatLabel>
      <Message v-if="$form.roll?.invalid" severity="error" size="small" variant="simple">
        {{ $form.roll.error.message }}
      </Message>
    </div>
    <Button :loading="loading" fluid label="Einloggen" severity="secondary" type="submit"/>
  </Form>
</template>

<style scoped>

</style>
