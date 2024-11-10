import React from 'react';
import { Link } from 'react-router-dom';

export const Navigation: React.FC = () => {
  return (
    <nav className="bg-white shadow-lg">
      <div className="max-w-7xl mx-auto px-4">
        <div className="flex justify-between h-16">
          <div className="flex">
            <div className="flex-shrink-0 flex items-center">
              <Link to="/" className="flex items-center gap-2">
                <div className="w-8 h-8 bg-gradient-to-r from-accent-purple to-accent-blue rounded-lg flex items-center justify-center">
                  <span className="text-white font-bold text-xl">M</span>
                </div>
                <span className="text-xl font-bold bg-gradient-to-r from-accent-purple to-accent-blue bg-clip-text text-transparent">
                  MedTrain AI
                </span>
              </Link>
            </div>
            <div className="hidden sm:ml-6 sm:flex sm:space-x-8">
              <Link to="/modules" className="nav-link inline-flex items-center px-1 pt-1 border-b-2 border-transparent hover:border-accent-purple">
                Learning Modules
              </Link>
              <Link to="/clinical-skills" className="nav-link inline-flex items-center px-1 pt-1 border-b-2 border-transparent hover:border-accent-purple">
                Clinical Skills
              </Link>
              <Link to="/assessment" className="nav-link inline-flex items-center px-1 pt-1 border-b-2 border-transparent hover:border-accent-purple">
                Assessment
              </Link>
              <Link to="/resources" className="nav-link inline-flex items-center px-1 pt-1 border-b-2 border-transparent hover:border-accent-purple">
                Resources
              </Link>
              <Link to="/community" className="nav-link inline-flex items-center px-1 pt-1 border-b-2 border-transparent hover:border-accent-purple">
                Community
              </Link>
            </div>
          </div>
          <div className="flex items-center">
            <Link to="/dashboard" className="nav-link">
              <div className="w-8 h-8 rounded-full bg-gray-200 flex items-center justify-center">
                <svg className="w-5 h-5 text-gray-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
                </svg>
              </div>
            </Link>
          </div>
        </div>
      </div>
    </nav>
  );
};

export default Navigation;