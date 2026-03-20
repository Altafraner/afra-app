import { computed, ref, shallowRef, toValue } from 'vue';
import { useSignalR } from '@/composables/signalr';
import type {
    AttendanceEvent,
    AttendanceEventWithEnrollments,
    AttendanceState,
    AttendanceStudentStatus,
} from '@/Attendance/models/attendance';
import type { Note } from '@/Attendance/models/note';
import { isNowInDateTimeInterval } from '@/helpers/time.js';

interface Capabilities {
    enableNotes: boolean;
    enableMove: boolean;
    moveNowInterval?: {
        start: string;
        end: string;
        duration: string;
    };
}

interface AttendanceUpdate {
    studentId: string;
    eventId: string;
    status: AttendanceState;
}

interface TerminStatusUpdate {
    eventId: string;
    status: boolean;
}

interface NoteUpdate {
    studentId: string;
    notes: Note[];
}

/**
 * Connects to the attendance hub and returns a reactive list of attendances.
 */
export function useAttendance(
    attendanceType: 'event' | 'slot',
    scope: string,
    slotId: string,
    toastService: { add: (message: any) => void } = { add: () => undefined },
    eventId?: string,
) {
    const {
        connectionPromise,
        registerMessageHandler,
        registerReconnectHandler,
        sendMessage,
        closeConnection,
    } = useSignalR('/api/attendance/hub', true, toastService);
    const slotAttendances = ref<AttendanceEventWithEnrollments[]>([]);
    const eventAttendances = ref<AttendanceStudentStatus[] | null>(null);
    const alternatives = shallowRef<AttendanceEvent[]>([]);
    const capabilities = shallowRef<Capabilities>();

    function canMoveNowNow(): boolean {
        return isNowInDateTimeInterval(capabilities.value?.moveNowInterval);
    }

    const canMoveNow = computed(() => canMoveNowNow());

    registerReconnectHandler(registerScope);
    connectionPromise.then(registerScope);

    if (toValue(attendanceType) === 'event') {
        registerMessageHandler('UpdateEvent', updateEvent);
        registerMessageHandler('UpdateAttendance', updateAttendanceInEvent);
        registerMessageHandler('UpdateNote', updateNoteInEvent);
    } else if (toValue(attendanceType) === 'slot') {
        registerMessageHandler('UpdateSlot', updateSlot);
        registerMessageHandler('UpdateAttendance', updateAttendanceInSlot);
        registerMessageHandler('UpdateTerminStatus', updateTerminStatus);
        registerMessageHandler('UpdateNote', updateNoteInSlot);
    } else {
        throw Error(`Unrecognized scope: ${attendanceType}`);
    }
    registerMessageHandler('Notify', notify);

    async function registerScope() {
        if (toValue(attendanceType) === 'event') {
            if (!eventId) throw Error(`No event id supplied`);
            capabilities.value = await sendMessage('SubscribeToEvent', scope, slotId, eventId!);
            return;
        }
        capabilities.value = await sendMessage('SubscribeToSlot', scope, slotId);
    }

    function updateEvent(data: AttendanceStudentStatus[]) {
        eventAttendances.value = data;
    }

    function updateSlot(data: AttendanceEventWithEnrollments[]) {
        slotAttendances.value = data;
    }

    function updateTerminStatus(data: TerminStatusUpdate) {
        const index = slotAttendances.value.findIndex((t) => t.eventId === data.eventId);
        if (index !== -1) {
            slotAttendances.value[index].status = data.status;
        } else {
            console.warn(`Received status for non-existent event`, data);
        }
    }

    function updateAttendanceInEvent(data: AttendanceUpdate) {
        const index = eventAttendances.value.findIndex((a) => a.student.id === data.studentId);
        if (index !== -1) {
            eventAttendances.value[index].status = data.status;
        } else {
            console.warn('Received status for non-existent user', data);
        }
    }

    function updateNoteInEvent(data: NoteUpdate) {
        const index = eventAttendances.value.findIndex((a) => a.student.id === data.studentId);
        if (index !== -1) {
            eventAttendances.value[index].notes = data.notes;
        }
    }

    function updateNoteInSlot(data: NoteUpdate) {
        for (let i = 0; i < slotAttendances.value.length; i++) {
            const index = slotAttendances.value[i].enrollments.findIndex(
                (a) => a.student.id === data.studentId,
            );
            if (index !== -1) {
                slotAttendances.value[i].enrollments[index].notes = data.notes;
            }
        }
    }

    function updateAttendanceInSlot(data: AttendanceUpdate) {
        const index = slotAttendances.value.findIndex((t) => t.eventId === data.eventId);
        if (index !== -1) {
            const terminAttendances = slotAttendances.value[index].enrollments;
            const innerIndex = terminAttendances.findIndex(
                (a) => a.student.id === data.studentId,
            );
            if (innerIndex !== -1) {
                terminAttendances[innerIndex].status = data.status;
            } else {
                console.warn(
                    `Received status for non-existent student in existing event`,
                    data,
                );
            }
        } else console.warn(`Received student status for non-existent event`, data);
    }

    const severityMap = {
        Error: 'error',
        Warning: 'warn',
        Info: 'info',
    };

    function notify(data: {
        subject: string;
        body: string;
        severity: 'Error' | 'Warning' | 'Info';
    }) {
        toastService.add({
            summary: data.subject,
            detail: data.body,
            severity: severityMap[data.severity],
        });
    }

    /**
     * Sends an update to the attendance hub for a specific student.
     * @param studentId The ID of the student whose attendance is being updated.
     * @param status The new attendance status for the student.
     */
    async function sendAttendanceUpdate(
        studentId: string,
        status: AttendanceState,
    ): Promise<void> {
        await sendMessage('SetAttendanceStatus', studentId, status);
    }

    /**
     * Sends a status update for a specific termin or block.
     * @param eventId The id of the event to set the status of.
     * @param status The new status to set for the termin or block.
     */
    async function sendStatusUpdate(eventId: string, status: boolean): Promise<void> {
        await sendMessage('SetTerminStatus', eventId, status);
    }

    /**
     * Fetches alternative events available in the current slot
     */
    async function updateAlternatives(): Promise<void> {
        alternatives.value = await sendMessage('GetEventsAvailable');
    }

    /**
     * Moves a student to a different slot
     * @param studentId the id of the student to move
     * @param terminId the id of the event to move the student to
     */
    async function sendMove(studentId: string, terminId: string): Promise<void> {
        await sendMessage('MoveStudent', studentId, terminId);
    }

    /**
     * Moves a student to a different event only from the current time onward
     * @param studentId the id of the student to move
     * @param terminId the id of the termin the student should be moved to
     */
    async function sendMoveNow(studentId: string, terminId: string): Promise<void> {
        await sendMessage('MoveStudentNow', studentId, terminId);
    }

    /**
     * Closes the connection to the server
     */
    async function close(): Promise<void> {
        await closeConnection();
        slotAttendances.value = [];
        eventAttendances.value = [];
        console.log('Attendance hub connection closed.');
    }

    return {
        slotAttendance: slotAttendances,
        eventAttendance: eventAttendances,
        alternatives,
        capabilities,
        canMoveNow,
        canMoveNowNow,
        updateAttendance: sendAttendanceUpdate,
        updateStatus: sendStatusUpdate,
        updateAlternatives,
        moveStudent: sendMove,
        moveStudentNow: sendMoveNow,
        stop: close,
    };
}
