import { useCallback, useEffect, useState, type FormEvent } from 'react'
import {
  ApiError,
  createCategory,
  deleteCategory,
  getCategories,
  updateCategory,
  type Category,
} from './api'
import { CategoryIcon } from './CategoryIcon'

const iconOptions = ['feather', 'bike', 'car', 'bus', 'truck']

const emptyForm = { name: '', icon: 'car', minWeight: '' }

function rangeText(c: Category) {
  const min = c.minWeight.toLocaleString()
  return c.maxWeight === null
    ? `${min} kg and above`
    : `${min} kg to under ${c.maxWeight.toLocaleString()} kg`
}

export function CategoryAdmin() {
  const [categories, setCategories] = useState<Category[]>([])
  const [form, setForm] = useState(emptyForm)
  const [editingId, setEditingId] = useState<number | null>(null)
  const [errors, setErrors] = useState<string[]>([])
  const [saving, setSaving] = useState(false)

  const load = useCallback(async () => {
    try {
      setCategories(await getCategories())
    } catch (e) {
      setErrors(e instanceof ApiError ? e.errors : ['Failed to load categories.'])
    }
  }, [])

  useEffect(() => {
    load()
  }, [load])

  function startEdit(c: Category) {
    setEditingId(c.id)
    setForm({ name: c.name, icon: c.icon, minWeight: String(c.minWeight) })
    setErrors([])
  }

  function reset() {
    setEditingId(null)
    setForm(emptyForm)
    setErrors([])
  }

  async function handleSubmit(e: FormEvent) {
    e.preventDefault()
    setSaving(true)
    setErrors([])
    const body = {
      name: form.name,
      icon: form.icon,
      minWeight: form.minWeight === '' ? -1 : Number(form.minWeight),
    }
    try {
      if (editingId === null) await createCategory(body)
      else await updateCategory(editingId, body)
      reset()
      await load()
    } catch (err) {
      setErrors(err instanceof ApiError ? err.errors : ['Failed to save the category.'])
    } finally {
      setSaving(false)
    }
  }

  async function handleDelete(c: Category) {
    if (!window.confirm(`Delete the "${c.name}" category?`)) return
    setErrors([])
    try {
      await deleteCategory(c.id)
      if (editingId === c.id) reset()
      await load()
    } catch (err) {
      setErrors(err instanceof ApiError ? err.errors : ['Failed to delete the category.'])
    }
  }

  const inputClass = 'w-full rounded border border-gray-300 p-2'

  return (
    <section>
      <h2 className="mb-1 text-xl font-semibold">Categories</h2>
      <p className="mb-4 text-sm text-gray-600">
        A category covers its start weight up to, but not including, the next category's start. The
        lowest category must start at 0 kg. Each vehicle's category updates automatically when these
        change.
      </p>

      {errors.length > 0 && (
        <ul className="mb-3 list-disc rounded bg-red-50 p-3 pl-8 text-red-700" role="alert">
          {errors.map((m) => (
            <li key={m}>{m}</li>
          ))}
        </ul>
      )}

      <table className="mb-8 w-full border-collapse text-left">
        <thead>
          <tr className="border-b border-gray-300">
            <th className="p-2">Icon</th>
            <th className="p-2">Name</th>
            <th className="p-2">Weight range</th>
            <th className="p-2" />
          </tr>
        </thead>
        <tbody>
          {categories.map((c) => (
            <tr key={c.id} className="border-b border-gray-200">
              <td className="p-2">
                <CategoryIcon icon={c.icon} label={c.name} />
              </td>
              <td className="p-2">{c.name}</td>
              <td className="p-2">{rangeText(c)}</td>
              <td className="space-x-3 p-2 text-right">
                <button type="button" onClick={() => startEdit(c)} className="cursor-pointer text-blue-700 hover:underline">
                  Edit
                </button>
                <button type="button" onClick={() => handleDelete(c)} className="cursor-pointer text-red-700 hover:underline">
                  Delete
                </button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>

      <h3 className="mb-3 text-lg font-semibold">{editingId === null ? 'Add category' : 'Edit category'}</h3>
      <form onSubmit={handleSubmit} className="max-w-md space-y-4" noValidate>
        <label className="block">
          <span className="mb-1 block font-medium">Name</span>
          <input className={inputClass} value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} />
        </label>

        <fieldset>
          <legend className="mb-1 font-medium">Icon</legend>
          <div className="flex gap-2">
            {iconOptions.map((key) => (
              <label
                key={key}
                className={`cursor-pointer rounded border p-2 ${
                  form.icon === key ? 'border-blue-700 bg-blue-50 text-blue-700' : 'border-gray-300'
                }`}
              >
                <input
                  type="radio"
                  name="icon"
                  value={key}
                  checked={form.icon === key}
                  onChange={() => setForm({ ...form, icon: key })}
                  className="sr-only"
                />
                <CategoryIcon icon={key} />
              </label>
            ))}
          </div>
        </fieldset>

        <label className="block">
          <span className="mb-1 block font-medium">Starts at (kg)</span>
          <input
            className={inputClass}
            type="number"
            step="0.01"
            value={form.minWeight}
            onChange={(e) => setForm({ ...form, minWeight: e.target.value })}
          />
        </label>

        <div className="flex gap-3">
          <button
            type="submit"
            disabled={saving}
            className="cursor-pointer rounded bg-blue-700 px-4 py-2 font-semibold text-white disabled:opacity-50"
          >
            {saving ? 'Saving…' : editingId === null ? 'Add category' : 'Save changes'}
          </button>
          {editingId !== null && (
            <button type="button" onClick={reset} className="cursor-pointer rounded border border-gray-300 px-4 py-2">
              Cancel
            </button>
          )}
        </div>
      </form>
    </section>
  )
}
