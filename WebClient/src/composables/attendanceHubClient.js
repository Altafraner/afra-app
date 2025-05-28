import {onUnmounted, ref, toValue} from "vue";
import * as signalR from "@microsoft/signalr";

/**
 * Connects to the attendance hub and returns a reactive list of attendances.
 * @param {'termin'|'block'} scope
 * @param {string} id
 * @returns {Object}
 */
export function useAttendance(scope, id) {
  const connection = ref(null)
  const attendances = ref([])

  const con = new signalR.HubConnectionBuilder()
    .withUrl("/api/otium/attendance")
    .withAutomaticReconnect()
    .configureLogging(signalR.LogLevel.Information)
    .build();
  connection.value = con;
  con.onclose(error => {
    console.log("Connection closed: ", error);
  })
  con.onreconnected(registerScope);
  con.start()
    .then(() => registerScope())

  if (toValue(scope) === "termin") {
    con.on('UpdateTerminAttendances', updateTerminAttendances);
    con.on('UpdateAttendance', updateAttendanceInTermin);
  } else if (toValue(scope) === "block") {
    con.on('UpdateBlockAttendances', updateBlockAttendances);
    con.on('UpdateAttendance', updateAttendanceInBlock);
  } else {
    throw Error(`Unrecognized scope: ${scope}`);
  }

  onUnmounted(async () => {
    await connection.value.stop();
  })

  async function registerScope() {
    const methodName = toValue(scope) === "termin" ? "SubscribeToTermin" : "SubscribeToBlock";
    await con.invoke(methodName, toValue(id));
  }

  function updateTerminAttendances(data) {
    attendances.value = data;
  }

  function updateBlockAttendances(data) {
    attendances.value = data;
  }

  function updateAttendanceInTermin(data) {
    const index = attendances.value.findIndex(a => a.student.id === data.studentId);
    if (index !== -1) {
      attendances.value[index].anwesenheit = data.status;
    } else {
      console.warn("Received status for non-existent user", data);
    }
  }

  function updateAttendanceInBlock(data) {
    const index = attendances.value.findIndex(t => t.terminId === data.terminId);
    if (index !== -1) {
      const terminAttendances = attendances.value[index].einschreibungen
      const innerIndex = terminAttendances.findIndex(a => a.student.id === data.studentId)
      if (innerIndex !== -1) {
        terminAttendances[innerIndex].anwesenheit = data.status;
      } else {
        console.warn(`Received status for non-existent student in termin`, data);
      }
    } else
      console.warn(`Received status for non-existent termin`, data);
  }

  /**
   * Sends an update to the attendance hub for a specific student.
   * @param studentId The ID of the student whose attendance is being updated.
   * @param {'Fehlend' | 'Entschuldigt', 'Anwesend'} status The new attendance status for the student.
   */
  function sendUpdate(studentId, status) {
    const methodName = toValue(scope) === "termin" ? "SetStatusForTermin" : "SetStatusForBlock";
    connection.value.invoke(methodName, toValue(id), studentId, status)
      .catch(error => {
        console.error("Error sending update:", error);
      });
  }

  return {attendance: attendances, update: sendUpdate};
}
