<script setup>
import { Form } from '@primevue/forms';
import {InputText, FloatLabel, Password, Button, useToast} from "primevue";
import {ref} from "vue";
import {mande} from "mande";
import {useUser} from "@/stores/useUser.js";

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
  } catch(error) {
    if (error.response.status === 410) {
      toast.add({severity: "error", summary: "Fehler", detail: "Falsche E-Mail oder Passwort", life: 5000});
    } else {
      toast.add({severity: "error", summary: "Fehler", detail: "Ein unbekannter Fehler ist aufgetreten", life: 5000});
    }
  } finally {
    loading.value = false;
  }
}
</script>

<template>
  <Form @submit="submit" class="flex flex-col gap-6 mt-8">
    <FloatLabel variant="on">
      <InputText name="username" type="text" fluid id="eMail"/>
      <label for="eMail">E-Mail Adresse</label>
    </FloatLabel>
    <FloatLabel variant="on">
      <Password name="password" :feedback="false" fluid toggle-mask input-id="password"/>
      <label for="password">Passwort</label>
    </FloatLabel>
    <Button type="submit" severity="secondary" label="Submit" fluid :loading="loading"/>
  </Form>
</template>

<style scoped>

</style>
