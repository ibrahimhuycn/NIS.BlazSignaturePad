import SignaturePad from "./signature_pad.js";

const instances = new Map();

export function init(id, dotNetRef, optionsJson) {
    const canvas = document.getElementById("sig-" + id);
    if (!canvas) throw new Error("Canvas #sig-" + id + " not found");

    const options = optionsJson ? JSON.parse(optionsJson) : {};
    const pad = new SignaturePad(canvas, options);

    const instance = {
        pad,
        dotNetRef,
        undoStack: [],
        redoStack: [],
        resizeHandler: null,
    };

    instance.resizeHandler = () => {
        const ratio = Math.max(window.devicePixelRatio || 1, 1);
        canvas.width = canvas.offsetWidth * ratio;
        canvas.height = canvas.offsetHeight * ratio;
        const ctx = canvas.getContext("2d");
        ctx.scale(ratio, ratio);
        pad.redraw();
    };

    window.addEventListener("resize", instance.resizeHandler);
    instance.resizeHandler();

    pad.addEventListener("beginStroke", () => {
        dotNetRef.invokeMethodAsync("NotifyBeginStroke");
    });

    pad.addEventListener("endStroke", () => {
        instance.redoStack = [];
        dotNetRef.invokeMethodAsync("NotifyEndStroke");
        dotNetRef.invokeMethodAsync("NotifyIsEmptyChanged", pad.isEmpty());
    });

    instances.set(id, instance);
}

export function destroy(id) {
    const inst = instances.get(id);
    if (!inst) return;
    window.removeEventListener("resize", inst.resizeHandler);
    inst.pad.off();
    instances.delete(id);
}

export function clear(id) {
    const inst = instances.get(id);
    if (!inst) return;
    inst.pad.clear();
    inst.undoStack = [];
    inst.redoStack = [];
    inst.dotNetRef.invokeMethodAsync("NotifyIsEmptyChanged", inst.pad.isEmpty());
}

export function isEmpty(id) {
    const inst = instances.get(id);
    return inst ? inst.pad.isEmpty() : true;
}

export function undo(id) {
    const inst = instances.get(id);
    if (!inst) return;
    const data = inst.pad.toData();
    if (data && data.length > 0) {
        const removed = data.pop();
        inst.undoStack.push(removed);
        inst.pad.fromData(data, { clear: true });
        inst.dotNetRef.invokeMethodAsync("NotifyIsEmptyChanged", inst.pad.isEmpty());
    }
}

export function redo(id) {
    const inst = instances.get(id);
    if (!inst) return;
    if (inst.undoStack.length > 0) {
        const data = inst.pad.toData();
        data.push(inst.undoStack.pop());
        inst.pad.fromData(data, { clear: true });
        inst.dotNetRef.invokeMethodAsync("NotifyIsEmptyChanged", inst.pad.isEmpty());
    }
}

export function setPenColor(id, color) {
    const inst = instances.get(id);
    if (inst) inst.pad.penColor = color;
}

export function setBackgroundColor(id, color) {
    const inst = instances.get(id);
    if (inst) {
        inst.pad.backgroundColor = color;
        inst.pad.redraw();
    }
}

export function setPenWidth(id, min, max) {
    const inst = instances.get(id);
    if (inst) {
        inst.pad.minWidth = Math.min(min, max);
        inst.pad.maxWidth = Math.max(min, max);
    }
}

export function toDataURL(id, type, encoderOptions) {
    const inst = instances.get(id);
    if (!inst) return null;
    return inst.pad.toDataURL(type, encoderOptions != null ? encoderOptions : undefined);
}

export function toByteArray(id, type, encoderOptions) {
    const inst = instances.get(id);
    if (!inst) return null;
    const dataUrl = inst.pad.toDataURL(type, encoderOptions != null ? encoderOptions : undefined);
    const parts = dataUrl.split(';base64,');
    const raw = window.atob(parts[1]);
    const uInt8Array = new Uint8Array(raw.length);
    for (let i = 0; i < raw.length; ++i) {
        uInt8Array[i] = raw.charCodeAt(i);
    }
    return new Blob([uInt8Array]);
}
