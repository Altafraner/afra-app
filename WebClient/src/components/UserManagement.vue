<script setup>
import {ref} from 'vue'
import CreateUserDialog from "@/components/UserManagement/CreateUserDialog.vue";
import UserOptions from "@/components/UserManagement/UserOptions.vue";

const itemsPerPage = ref(5)
const headers = ref([
  {
    title: "Nachname",
    align: 'end',
    sortable: true,
    key: "lastName",
  },
  {
    title: "Vorname",
    align: 'end',
    sortable: true,
    key: "firstName",
  },
  {
    title: "Gymnasialmentor",
    align: 'end',
    sortable: false,
    key: "mentor.name",
  },
  {
    title: "Klasse",
    align: 'start',
    sortable: true,
    key: "class.name",
  },
  {
    title: "",
    align: 'center',
    sortable: false,
    key: "action",
  },
])

async function fetchUsersPaginated(page, pageSize, OrderBy, OrderDirection, filterElement) {
  const response = await fetch(`http://localhost:5043/api/Person/page?page=${page}&pageSize=${pageSize}&sort=${OrderBy}&sortDirection=${OrderDirection}`, {
    method: "POST",
    body: JSON.stringify(filterElement),
    headers: {
      "Content-Type": "application/json",
    }
  });

  if (!response.ok) throw Error("Well, this isn't the data we wanted")

  return response.json();
}

async function fetchClasses() {
  const response = await fetch(`http://localhost:5043/api/Class`);
  const json = await response.json();
  console.log(json)
  return json.map((obj) => {
    return {
      title: obj.level.toString().padStart(2, "0") + " " + obj.appendix,
      value: obj.id
    }
  })
}

function filter(evt) {
  filterElement.value = {
    firstName: filterFirstName.value,
    lastName: filterLastName.value,
    classes: filterClasses.value
  }

  loadItems({page: currentPage.value, itemsPerPage: itemsPerPage.value, sortBy: sorting.value})
}

const reloadTable = () => loadItems({page: currentPage.value, itemsPerPage: itemsPerPage.value, sortBy: sorting.value});

const currentPage = ref(1)
const search = ref('')
const sorting = ref([])
const serverItems = ref([])
const loading = ref(true)
const totalItems = ref(0)
const classes = ref([])
const filterFirstName = ref("")
const filterLastName = ref("")
const filterClasses = ref([])
const filterElement = ref({
  firstName: "",
  lastName: "",
  classes: []
})
const selected = ref([])
const deleteDialog = ref(false)
const deleteInProgress = ref(false)

async function loadItems({page, itemsPerPage, sortBy}) {
  loading.value = true
  sorting.value = sortBy

  let sort_column = "lastName"
  let sort_direction = "asc"

  if (sortBy.length) {
    sort_column = sortBy[0].key
    sort_direction = sortBy[0].order
  }

  const data = await fetchUsersPaginated(page, itemsPerPage, sort_column, sort_direction, filterElement.value)

  serverItems.value = data.items
  totalItems.value = data.total
  loading.value = false
}

fetchClasses()
  .then((obj) => {
    classes.value = obj
    classes.value = classes.value.sort((a, b) => a.title.localeCompare(b.title))
    classes.value.unshift({title: "Keine Klasse", value: null})
  })

async function deleteConfirm(evt) {
  deleteInProgress.value = true
  let operations = []

  for (const userId of selected.value) {
    operations.push(
      fetch(`http://localhost:5043/api/Person/${userId}`, {
        method: "DELETE"
      })
    )
  }

  for (const operation of operations) {
    await operation
  }

  await reloadTable()
  deleteDialog.value = false
  deleteInProgress.value = false
  selected.value = []
}

</script>

<template>
  <v-container>
    <h1>Nutzerverwaltung</h1>
    <v-expansion-panels flat>
      <v-expansion-panel
        color="grey-lighten-3"
      >
        <v-expansion-panel-title>
          Filter
        </v-expansion-panel-title>
        <v-expansion-panel-text>
          <v-form @submit.prevent="filter">
            <p class="text-subtitle-1">Name</p>
            <div class="d-flex ga-4">
              <v-text-field v-model="filterLastName" label="Nachname" density="compact" clearable></v-text-field>
              <v-text-field v-model="filterFirstName" label="Vorname" density="compact" clearable></v-text-field>
            </div>
            <p class="text-subtitle-1">Klasse</p>
            <v-select
              v-model="filterClasses"
              label="Klasse"
              :items="classes"
              multiple
              density="compact"
              clearable
            ></v-select>
            <v-btn class="mt-2 mb-4" text="Anwenden" type="submit" block color="primary"/>
          </v-form>
        </v-expansion-panel-text>
      </v-expansion-panel>
    </v-expansion-panels>

    <v-data-table-server
      v-model:items-per-page="itemsPerPage"
      :headers="headers"
      :items="serverItems"
      :items-length="totalItems"
      :loading="loading"
      :search="search"
      :page="currentPage"
      @update:options="loadItems"
      density="compact"
      show-select
      v-model="selected"
    >
      <template v-slot:item.action="{item}">
        <user-options :item="item" @user-deleted="reloadTable"/>
      </template>
    </v-data-table-server>
    <v-toolbar density="compact" color="transparent">
      <template v-slot:prepend>
        <v-dialog
          max-width="400"
          v-model="deleteDialog">
          <template v-slot:activator="{ props: activatorProps }">
            <v-btn icon="mdi-delete-outline" v-bind="activatorProps" :disabled="selected.length===0"
                   variant="text" class="text-none">
            </v-btn>
          </template>

          <v-card prepend-icon="mdi-alert"
                  title="Sind Sie sicher?">
            <v-card-text>
              <p>
                Wollen sie wirklich {{ selected.length === 1 ? "einen" : selected.length }} Nutzer löschen?
              </p>
            </v-card-text>
            <v-card-actions>
              <v-btn @click="deleteDialog=false" class="text-none">Abbrechen</v-btn>
              <v-btn @click="deleteConfirm" color="primary" variant="flat" :loading="deleteInProgress"
                     class="text-none">Löschen
              </v-btn>
            </v-card-actions>
          </v-card>
        </v-dialog>
      </template>
      <create-user-dialog :classes="classes" @user-created="reloadTable"/>
    </v-toolbar>
  </v-container>
</template>

<style scoped>

</style>
