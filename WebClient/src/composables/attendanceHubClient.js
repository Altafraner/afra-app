import {ref, toValue} from "vue";
import {useSignalR} from "@/composables/signalr.js";

/**
 * Connects to the attendance hub and returns a reactive list of attendances.
 * @param {'termin'|'block'} scope
 * @param {string} id
 * @param toastService
 * @returns {Object}
 */
export function useAttendance(scope, id, toastService = {add: () => undefined}) {
  const {
    connectionPromise,
    registerMessageHandler,
    registerReconnectHandler,
    sendMessage
  } = useSignalR('/api/otium/attendance', true, toastService)
  const attendances = ref([])

  registerReconnectHandler(registerScope)
  connectionPromise.then(registerScope)

  if (toValue(scope) === "termin") {
    registerMessageHandler('UpdateTerminAttendances', updateTerminAttendances);
    registerMessageHandler('UpdateAttendance', updateAttendanceInTermin);
  } else if (toValue(scope) === "block") {
    registerMessageHandler('UpdateBlockAttendances', updateBlockAttendances);
    registerMessageHandler('UpdateAttendance', updateAttendanceInBlock);
    registerMessageHandler('UpdateTerminStatus', updateTerminStatus);
  } else {
    throw Error(`Unrecognized scope: ${scope}`);
  }

  async function registerScope() {
    const methodName = toValue(scope) === "termin" ? "SubscribeToTermin" : "SubscribeToBlock";
    await sendMessage(methodName, toValue(id));
  }

  function updateTerminAttendances(data) {
    attendances.value = data;
  }

  function updateBlockAttendances(data) {
    attendances.value = data;
  }

  function updateTerminStatus(data) {
    const index = attendances.value.findIndex(t => t.terminId === data.terminId);
    if (index !== -1) {
      attendances.value[index].sindAnwesenheitenErfasst = data.sindAnwesenheitenErfasst;
    } else {
      console.warn(`Received status for non-existent termin`, data);
    }
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
  async function sendAttendanceUpdate(studentId, status) {
    const methodName = toValue(scope) === "termin" ? "SetAttendanceStatusInTermin" : "SetAttendanceStatusInBlock";
    await sendMessage(methodName, toValue(id), studentId, status)
  }

  /**
   * Sends a status update for a specific termin or block.
   * @param innerId If the scope is "termin", this is the id of the block, otherwise it's the id of the termin.
   * @param status The new status to set for the termin or block.
   */
  async function sendStatusUpdate(innerId, status) {
    const blockId = toValue(scope) === "termin" ? innerId : toValue(id)
    const terminId = toValue(scope) === "termin" ? toValue(id) : innerId
    await sendMessage('SetTerminStatus', blockId, terminId, status)
  }

  async function sendMove(studentId, terminId) {
    await sendMessage('MoveStudent', studentId, terminId)
  }

  async function sendMoveNow(studentId, fromTerminId, toTerminId) {
    await sendMessage('MoveStudentNow', studentId, fromTerminId, toTerminId)
  }

  return {
    attendance: attendances,
    updateAttendance: sendAttendanceUpdate,
    updateStatus: sendStatusUpdate,
    moveStudent: sendMove,
    moveStudentNow: sendMoveNow
  };
}
