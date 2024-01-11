import { AtomEffect, DefaultValue } from "recoil"

export function synchronizeWithLocalStorage<T>(key: string, defaultValue: T): AtomEffect<T> {
  return ({ setSelf, onSet, trigger }) => {
    const savedValue = localStorage.getItem(key)
    if (savedValue != null) {
      setSelf(JSON.parse(savedValue))
    } else if (trigger == "get") {
      setSelf(defaultValue)
      localStorage.setItem(key, JSON.stringify(defaultValue))
    }

    onSet((newValue: T, _: T | DefaultValue, isReset: boolean) => {
      isReset ?? !newValue
        ? localStorage.removeItem(key)
        : localStorage.setItem(key, JSON.stringify(newValue))
    })
  }
}
