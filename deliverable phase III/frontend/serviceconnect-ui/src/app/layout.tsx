import type { Metadata } from 'next'
import { Inter } from 'next/font/google'
import '../styles/globals.css'
import Link from 'next/link'

const inter = Inter({ subsets: ['latin'] })

export const metadata: Metadata = {
  title: 'ServiceConnect - Phase 3',
  description: 'On-demand service worker marketplace with dual BLL implementations',
}

export default function RootLayout({
  children,
}: {
  children: React.ReactNode
}) {
  return (
    <html lang="en">
      <body className={inter.className}>
        <nav className="bg-primary-600 text-white p-4">
          <div className="container mx-auto flex justify-between items-center">
            <Link href="/" className="text-2xl font-bold">
              ServiceConnect
            </Link>
            <div className="space-x-4">
              <Link href="/jobs" className="hover:text-primary-200">Jobs</Link>
              <Link href="/workers" className="hover:text-primary-200">Workers</Link>
              <Link href="/customers" className="hover:text-primary-200">Customers</Link>
              <Link href="/bids" className="hover:text-primary-200">Bids</Link>
              <Link href="/bookings" className="hover:text-primary-200">Bookings</Link>
              <Link href="/analytics" className="hover:text-primary-200">Analytics</Link>
              <Link href="/settings" className="hover:text-primary-200">Settings</Link>
            </div>
          </div>
        </nav>
        <main className="container mx-auto p-4">
          {children}
        </main>
        <footer className="bg-gray-800 text-white p-4 mt-8">
          <div className="container mx-auto text-center">
            <p>ServiceConnect - Database Project Phase 3</p>
            <p className="text-sm text-gray-400">Dual BLL Implementation (LINQ/EF & Stored Procedures)</p>
          </div>
        </footer>
      </body>
    </html>
  )
}