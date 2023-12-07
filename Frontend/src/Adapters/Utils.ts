import { AtomEffect, DefaultValue } from "recoil"

export function synchronizeWithLocalStorage<T>(key: string): AtomEffect<T> {
  return ({ setSelf, onSet }) => {
    const savedValue = localStorage.getItem(key)
    if (savedValue != null) {
      setSelf(JSON.parse(savedValue))
    }

    onSet((newValue: T, _: T | DefaultValue, isReset: boolean) => {
      isReset ?? !newValue
        ? localStorage.removeItem(key)
        : localStorage.setItem(key, JSON.stringify(newValue))
    })
  }
}
