import { ref, toValue } from 'vue';
import { useSignalR } from '@/composables/signalr.js';

/**
 * Connects to the attendance hub and returns a reactive list of attendances.
 * @param {'termin'|'block'} scope
 * @param {string} id
 * @param toastService
 * @returns {Object}
 */
export function useAttendance(scope, id, toastService = { add: () => undefined }) {
    const {
        connectionPromise,
        registerMessageHandler,
        registerReconnectHandler,
        sendMessage,
        closeConnection,
    } = useSignalR('/api/otium/attendance', true, toastService);
    const attendances = ref([]);
    const alternatives = ref([]);
    let autoRecallIntervall = null;

    registerReconnectHandler(registerScope);
    connectionPromise.then(registerScope).then(registerAutoRecall);

    if (toValue(scope) === 'termin') {
        registerMessageHandler('UpdateTerminAttendances', updateTerminAttendances);
        registerMessageHandler('UpdateAttendance', updateAttendanceInTermin);
    } else if (toValue(scope) === 'block') {
        registerMessageHandler('UpdateBlockAttendances', updateBlockAttendances);
        registerMessageHandler('UpdateAttendance', updateAttendanceInBlock);
        registerMessageHandler('UpdateTerminStatus', updateTerminStatus);
    } else {
        throw Error(`Unrecognized scope: ${scope}`);
    }
    registerMessageHandler('Notify', notify);

    async function registerScope() {
        const methodName =
            toValue(scope) === 'termin' ? 'SubscribeToTermin' : 'SubscribeToBlock';
        await sendMessage(methodName, toValue(id));
    }

    function registerAutoRecall() {
        autoRecallIntervall = setInterval(registerScope, 1000 * 60 * 5);
    }

    function updateTerminAttendances(data) {
        attendances.value = data;
    }

    function updateBlockAttendances(data) {
        attendances.value = data;
    }

    function updateTerminStatus(data) {
        const index = attendances.value.findIndex((t) => t.terminId === data.terminId);
        if (index !== -1) {
            attendances.value[index].sindAnwesenheitenErfasst = data.sindAnwesenheitenErfasst;
        } else {
            console.warn(`Received status for non-existent termin`, data);
        }
    }

    function updateAttendanceInTermin(data) {
        const index = attendances.value.findIndex((a) => a.student.id === data.studentId);
        if (index !== -1) {
            attendances.value[index].anwesenheit = data.status;
        } else {
            console.warn('Received status for non-existent user', data);
        }
    }

    function updateAttendanceInBlock(data) {
        const index = attendances.value.findIndex((t) => t.terminId === data.terminId);
        if (index !== -1) {
            const terminAttendances = attendances.value[index].einschreibungen;
            const innerIndex = terminAttendances.findIndex(
                (a) => a.student.id === data.studentId,
            );
            if (innerIndex !== -1) {
                terminAttendances[innerIndex].anwesenheit = data.status;
            } else {
                console.warn(`Received status for non-existent student in termin`, data);
            }
        } else console.warn(`Received status for non-existent termin`, data);
    }

    const severityMap = {
        Error: 'error',
        Warning: 'warn',
        Info: 'info',
    };

    function notify(data) {
        toastService.add({
            summary: data.subject,
            detail: data.body,
            severity: severityMap[data.severity],
        });
    }

    /**
     * Sends an update to the attendance hub for a specific student.
     * @param studentId The ID of the student whose attendance is being updated.
     * @param {'Fehlend' | 'Entschuldigt', 'Anwesend'} status The new attendance status for the student.
     */
    async function sendAttendanceUpdate(studentId, status) {
        const methodName =
            toValue(scope) === 'termin'
                ? 'SetAttendanceStatusInTermin'
                : 'SetAttendanceStatusInBlock';
        await sendMessage(methodName, toValue(id), studentId, status);
    }

    /**
     * Sends a status update for a specific termin or block.
     * @param innerId If the scope is "termin", this is the id of the block, otherwise it's the id of the termin.
     * @param status The new status to set for the termin or block.
     */
    async function sendStatusUpdate(innerId, status) {
        const blockId = toValue(scope) === 'termin' ? innerId : toValue(id);
        const terminId = toValue(scope) === 'termin' ? toValue(id) : innerId;
        await sendMessage('SetTerminStatus', blockId, terminId, status);
    }

    async function updateAlternatives() {
        if (scope !== 'termin')
            throw Error('You can only update alternatives for a termin, not a block.');
        alternatives.value = await sendMessage('GetTerminAlternatives', id);
    }

    async function sendMove(studentId, terminId) {
        await sendMessage('MoveStudent', studentId, terminId);
    }

    async function sendUnenroll(studentId, terminId) {
        await sendMessage('ForceUnenroll', studentId, terminId);
    }

    async function sendMoveNow(studentId, fromTerminId, toTerminId) {
        await sendMessage('MoveStudentNow', studentId, fromTerminId, toTerminId);
    }

    async function close() {
        clearInterval(autoRecallIntervall);
        await closeConnection();
        attendances.value = [];
        console.log('Attendance hub connection closed.');
    }

    return {
        attendance: attendances,
        alternatives,
        updateAttendance: sendAttendanceUpdate,
        updateStatus: sendStatusUpdate,
        updateAlternatives,
        moveStudent: sendMove,
        moveStudentNow: sendMoveNow,
        unenroll: sendUnenroll,
        stop: close,
    };
}
