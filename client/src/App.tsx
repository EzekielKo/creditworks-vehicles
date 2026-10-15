import { useState } from 'react'
import { AddVehicleForm } from './AddVehicleForm'
import { VehicleList } from './VehicleList'

type Tab = 'vehicles' | 'add'

const tabs: { key: Tab; label: string }[] = [
  { key: 'vehicles', label: 'Vehicles' },
  { key: 'add', label: 'Add vehicle' },
]

export default function App() {
  const [tab, setTab] = useState<Tab>('vehicles')

  return (
    <main className="mx-auto max-w-4xl p-6">
      <h1 className="mb-6 text-3xl font-bold">Vehicle Manager</h1>
      <nav className="mb-6 flex gap-2 border-b border-gray-300">
        {tabs.map((t) => (
          <button
            key={t.key}
            type="button"
            onClick={() => setTab(t.key)}
            className={`cursor-pointer px-4 py-2 font-medium ${
              tab === t.key ? 'border-b-2 border-blue-700 text-blue-700' : 'text-gray-600'
            }`}
          >
            {t.label}
          </button>
        ))}
      </nav>
      {tab === 'vehicles' && <VehicleList />}
      {tab === 'add' && <AddVehicleForm onAdded={() => setTab('vehicles')} />}
    </main>
  )
}
