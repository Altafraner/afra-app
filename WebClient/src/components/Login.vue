<script setup>
import {Form} from '@primevue/forms';
import {Button, FloatLabel, InputText, Password, useToast} from "primevue";
import {ref} from "vue";
import {mande} from "mande";
import {useUser} from "@/stores/user.js";

const loading = ref(false)
const user = useUser()
const toast = useToast()

const submit = async (evt) => {
  if (loading.value) return;
  const username = evt.states['username'].value;
  const password = evt.states['password'].value;
  if (!(username && password)) return;
  loading.value = true;
  try {
    const loginRequest = await mande('/api/user/login').post({
      username: username,
      password: password
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
</script>

<template>
  <Form @submit="submit" class="flex flex-col gap-6 mt-8">
    <FloatLabel variant="on">
      <InputText id="username" fluid name="username" type="text"/>
      <label for="username">Nutzername</label>
    </FloatLabel>
    <FloatLabel variant="on">
      <Password name="password" :feedback="false" fluid toggle-mask input-id="password"/>
      <label for="password">Passwort</label>
    </FloatLabel>
    <Button :loading="loading" fluid label="Einloggen" severity="secondary" type="submit"/>
  </Form>
</template>

<style scoped>

</style>
